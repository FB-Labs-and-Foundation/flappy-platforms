using System;
using System.Collections.Generic;
using System.Linq;
using Plugins.Storage.Core.Runtime;
// #if !UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine;

// #endif

namespace Playgama.Modules.Storage
{
    public class PlaygamaStorageService : IStorageService
    {
        #region INTERNAL

        #if !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string PlaygamaBridgeIsStorageSupported(string storageType);

        [DllImport("__Internal")]
        private static extern string PlaygamaBridgeIsStorageAvailable(string storageType);

        [DllImport("__Internal")]
        private static extern string PlaygamaBridgeGetStorageDefaultType();

        [DllImport("__Internal")]
        private static extern void PlaygamaBridgeGetStorageData(string key, string storageType);

        [DllImport("__Internal")]
        private static extern void PlaygamaBridgeSetStorageData(string key, string value, string storageType);

        [DllImport("__Internal")]
        private static extern void PlaygamaBridgeDeleteStorageData(string key, string storageType);
        #endif

        #endregion
        
        #if !UNITY_EDITOR
        public StorageType DefaultType 
        { 
           get
           {
               var type = PlaygamaBridgeGetStorageDefaultType();
               return ParseStorageType(type);
           }
        }
        #else
        public StorageType DefaultType => StorageType.LocalStorage;
        #endif
        
        private const string _dataSeparator = "{bridge_data_separator}";
        private const string _keysSeparator = "{bridge_keys_separator}";
        private const string _valuesSeparator = "{bridge_values_separator}";
        
        private readonly Dictionary<string, List<Action<bool, string>>> _getDataCallbacks = new();
        private readonly Dictionary<string, List<Action<bool>>> _setDataCallbacks = new();
        private readonly Dictionary<string, List<Action<bool>>> _removeDataCallbacks = new();
        
        public bool IsSupported(StorageType storageType)
        {
#if !UNITY_EDITOR
            return PlaygamaBridgeIsStorageSupported(ConvertStorageType(storageType)) == "true";
#else
            return storageType == StorageType.LocalStorage;
#endif
        }

        public bool IsAvailable(StorageType storageType)
        {
#if !UNITY_EDITOR
            return PlaygamaBridgeIsStorageAvailable(ConvertStorageType(storageType)) == "true";
#else
            return storageType == StorageType.LocalStorage;
#endif
        }

        public void Get(string key, Action<bool, string> onComplete, StorageType? storageType = null)
        {
            if (_getDataCallbacks.TryGetValue(key, out var callbacks))
            {
                callbacks.Add(onComplete);
                _getDataCallbacks[key] = callbacks;
            }
            else
            {
                _getDataCallbacks.Add(key, new List<Action<bool, string>> { onComplete });
#if !UNITY_EDITOR
                PlaygamaBridgeGetStorageData(key, ConvertStorageType(storageType));
#else
                var data = PlayerPrefs.GetString(key, null);
                OnGetStorageDataSuccess($"{key}{_dataSeparator}{data}");
#endif
            }
        }

        public void Set(string key, string value, Action<bool> onComplete = null, StorageType? storageType = null)
        {
            if (_setDataCallbacks.TryGetValue(key, out var callbacks))
            {
                callbacks.Add(onComplete);
                _setDataCallbacks[key] = callbacks;
            }
            else
            {
                _setDataCallbacks.Add(key, new List<Action<bool>> { onComplete });
#if !UNITY_EDITOR
                PlaygamaBridgeSetStorageData(key, value, ConvertStorageType(storageType));
#else
                PlayerPrefs.SetString(key, value);
                OnSetStorageDataSuccess(key);
#endif
            }
        }

        public void Set(string key, int value, Action<bool> onComplete = null, StorageType? storageType = null)
        {
            Set(key, value.ToString(), onComplete, storageType);
        }

        public void Set(string key, bool value, Action<bool> onComplete = null, StorageType? storageType = null)
        {
            Set(key, value.ToString(), onComplete, storageType);
        }

        public void Remove(string key, Action<bool> onComplete = null, StorageType? storageType = null)
        {
            if (_removeDataCallbacks.TryGetValue(key, out var callbacks))
            {
                callbacks.Add(onComplete);
                _removeDataCallbacks[key] = callbacks;
            }
            else
            {
                _removeDataCallbacks.Add(key, new List<Action<bool>> { onComplete });
#if !UNITY_EDITOR
                PlaygamaBridgeDeleteStorageData(key, ConvertStorageType(storageType));
#else
                PlayerPrefs.DeleteKey(key);
                OnDeleteStorageDataSuccess(key);
#endif
            }
        }

        #region CALLBACKS

        // Called from JS
        private void OnGetStorageDataSuccess(string result)
        {
            var keyEndIndex = result.IndexOf(_dataSeparator);
            
            if (keyEndIndex <= 0)
                return;

            var keysString = result.Substring(0, keyEndIndex);
            var valuesString = result.Substring(keyEndIndex + _dataSeparator.Length, result.Length - keyEndIndex - _dataSeparator.Length);
            
            if (_getDataCallbacks.TryGetValue(keysString, out var callbacks))
            {
                _getDataCallbacks.Remove(keysString);

                foreach (var callback in callbacks)
                    callback?.Invoke(true, string.IsNullOrEmpty(valuesString) ? null : valuesString);
            }
        }

        private void OnGetStorageDataFailed(string keysString)
        {
            if (_getDataCallbacks.TryGetValue(keysString, out var callbacks))
            {
                _getDataCallbacks.Remove(keysString);

                foreach (var callback in callbacks)
                    callback?.Invoke(false, null);
            }
        }
        
        private void OnSetStorageDataSuccess(string key)
        {
            if (_setDataCallbacks.TryGetValue(key, out var callbacks))
            {
                _setDataCallbacks.Remove(key);

                foreach (var callback in callbacks)
                    callback?.Invoke(true);
            }
        }

        private void OnSetStorageDataFailed(string key)
        {
            if (_setDataCallbacks.TryGetValue(key, out var callbacks))
            {
                _setDataCallbacks.Remove(key);

                foreach (var callback in callbacks)
                    callback?.Invoke(false);
            }
        }

        private void OnDeleteStorageDataSuccess(string key)
        {
            if (_removeDataCallbacks.TryGetValue(key, out var callbacks))
            {
                _removeDataCallbacks.Remove(key);

                foreach (var callback in callbacks)
                    callback?.Invoke(true);
            }
        }

        private void OnDeleteStorageDataFailed(string key)
        {
            if (_removeDataCallbacks.TryGetValue(key, out var callbacks))
            {
                _removeDataCallbacks.Remove(key);

                foreach (var callback in callbacks)
                    callback?.Invoke(false);
            }
        }

        #endregion

        #region UTILITY

        private string ConvertStorageType(StorageType? storageType)
        {
            if (storageType.HasValue)
            {
                switch (storageType.Value)
                {
                    case StorageType.LocalStorage:
                        return "local_storage";

                    case StorageType.PlatformInternal:
                        return "platform_internal";

                    default:
                        throw new ArgumentOutOfRangeException(nameof(storageType), storageType, null);
                }
            }

            return "";
        }

        private StorageType ParseStorageType(string type)
        {
            switch (type)
            {
                case "local_storage":
                    return StorageType.LocalStorage;

                case "platform_internal":
                    return StorageType.PlatformInternal;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        #endregion
    }
}