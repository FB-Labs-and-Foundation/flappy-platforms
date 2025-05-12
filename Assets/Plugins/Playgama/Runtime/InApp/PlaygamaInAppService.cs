#if UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.Linq;
using InApp.Core.Runtime;
using UnityEngine;
#if !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Plugins.Playgama.Runtime.InApp
{
    public class PlaygamaInAppService : MonoBehaviour, IStoreService
    {
        private const string PRODUCT_ID = "commonId";
        private const string PRODUCT_PRICE = "price";
        private const string PRODUCT_CURRENCY = "priceCurrencyCode";
        private const string PRODUCT_PRICE_VALUE = "priceValue";
        
        public bool isSupported
        {
            get
            {
#if !UNITY_EDITOR
                return PlaygamaBridgeIsPaymentsSupported() == "true";
#else
                return false;
#endif
            }
        }

#if !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string PlaygamaBridgeIsPaymentsSupported();

        [DllImport("__Internal")]
        private static extern void PlaygamaBridgePaymentsPurchase(string id);

        [DllImport("__Internal")]
        private static extern void PlaygamaBridgePaymentsConsumePurchase(string id);
        
        [DllImport("__Internal")]
        private static extern void PlaygamaBridgePaymentsGetPurchases();
        
        [DllImport("__Internal")]
        private static extern void PlaygamaBridgePaymentsGetCatalog();
#endif
        
        private Action<bool, List<Dictionary<string, string>>> _getCatalogCallback;
        private Dictionary<string, Dictionary<string, string>> _cachedProducts = new();
        private IPurchaseHandler _purchaseHandler;
        private bool _isInitialized;
        private string _consumingProductId;
        
        public bool IsPurchaseInProcess { get; private set; }

        public void Initialize(IPurchaseHandler purchaseHandler)
        {
            _purchaseHandler = purchaseHandler;
            
            GetCatalog((success, products) =>
            {
                if (success)
                    foreach (var product in products)
                        _cachedProducts.Add(product[PRODUCT_ID], product);
                    
                _isInitialized = true;
            });
        }

        public bool IsInitialized()
        {
            return _isInitialized;
        }
        
        public void Purchase(string id)
        {
            IsPurchaseInProcess = true;
            
#if !UNITY_EDITOR
            PlaygamaBridgePaymentsPurchase(id);
#else
            OnPaymentsPurchaseFailed();
#endif
        }
        
        public decimal GetCost(string productId)
        {
            string res = GetProductField(productId, PRODUCT_PRICE_VALUE);

            if (decimal.TryParse(res, out var resDec))
                return resDec;

            return 0;
        }

        public string GetLocalizedCost(string productId)
        {
            return GetProductField(productId, PRODUCT_PRICE);
        }

        public string GetCurrency(string productId)
        {
            return GetProductField(productId, PRODUCT_CURRENCY);
        }

        public void ConfirmPurchaseReceiving(string productId)
        {
            _consumingProductId = productId;
            Consume(productId);
        }

        public void ProcessPendingPurchases()
        {
            GetPurchases();
        }
        
        private void GetPurchases(Action<bool, List<Dictionary<string, string>>> onComplete = null)
        {
#if !UNITY_EDITOR
            PlaygamaBridgePaymentsGetPurchases();
#else
            OnPaymentsGetPurchasesCompletedFailed();
#endif
        }
        
        private void GetCatalog(Action<bool, List<Dictionary<string, string>>> onComplete = null)
        {
            _getCatalogCallback = onComplete;

#if !UNITY_EDITOR
            PlaygamaBridgePaymentsGetCatalog();
#else
            OnPaymentsGetCatalogCompletedFailed();
#endif
        }
        
        private void Consume(string productId)
        {
#if !UNITY_EDITOR
            PlaygamaBridgePaymentsConsumePurchase(id);
#else
            OnPaymentsConsumePurchaseCompleted("false");
#endif
        }

        private string GetProductField(string productId, string fieldName)
        {
            if (string.IsNullOrEmpty(productId))
                return default;

            if (_isInitialized)
            {
                if (_cachedProducts.ContainsKey(productId))
                    return _cachedProducts[productId][fieldName];

                return default;
            }

            return default;
        }

        #region JS_CALLBACKS

        private void OnPaymentsPurchaseCompleted(string result)
        {
            var purchase = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    purchase = JsonHelper.FromJsonToDictionary(result);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            
            IsPurchaseInProcess = false;

            var firstPurchase = purchase.First();
            _purchaseHandler.OnPurchaseCompleted(firstPurchase.Key);
        }

        private void OnPaymentsPurchaseFailed()
        {
            Debug.Log("Purchase Failed");
            IsPurchaseInProcess = false;
        }
        
        private void OnPaymentsConsumePurchaseCompleted(string result)
        {
            var isSuccess = result == "true";
            
            if (isSuccess)
                _purchaseHandler.ConfirmPurchaseReceiving(_consumingProductId);
        }
        
        private void OnPaymentsGetPurchasesCompletedSuccess(string result)
        {
            var purchases = new List<Dictionary<string, string>>();

            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    purchases = JsonHelper.FromJsonToListOfDictionaries(result);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

            foreach (var purchase in purchases)
                _purchaseHandler.OnPurchaseCompleted(purchase[PRODUCT_ID], true);
        }

        private void OnPaymentsGetPurchasesCompletedFailed()
        {
            Debug.Log("Get purchases Failed");
        }
        
        private void OnPaymentsGetCatalogCompletedSuccess(string result)
        {
            var items = new List<Dictionary<string, string>>();

            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    items = JsonHelper.FromJsonToListOfDictionaries(result);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

            _getCatalogCallback?.Invoke(true, items);
            _getCatalogCallback = null;
        }

        private void OnPaymentsGetCatalogCompletedFailed()
        {
            _getCatalogCallback?.Invoke(false, null);
            _getCatalogCallback = null;
        }

        #endregion
    }
}
#endif