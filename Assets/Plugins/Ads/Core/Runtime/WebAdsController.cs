using System;
using System.Collections;
using System.Collections.Generic;
using Ads.Core.Runtime.AdStatus;
using UnityEngine;

namespace Ads.Core.Runtime
{
    public abstract class WebAdsController<TEnum> : AdsController<TEnum> where TEnum : Enum
    {
        protected TEnum _startedAd;
        protected readonly HashSet<TEnum> _interstitial = new();
        protected readonly HashSet<TEnum> _rewarded = new();

        private bool _adInProgress = false;
        public override bool AdInProgress => _adInProgress;
        private readonly Dictionary<TEnum, IEnumerator> _loaders = new();
        protected const float Timeout = 60f;
        protected override void Initialize(string sdkKey, string playerId, bool ageRestrictedFlag)
        {

        }

        public override bool IsInitialized()
        {
            return true;
        }

        public override void ShowRewardedAd(TEnum gamePlacement)
        {
            ShowAds(gamePlacement);

            if (Application.isEditor)
            {
                OnShowRewardedSuccessful();
                return;
            }

#if UNITY_WEBGL
            ShowRewarded();
#endif
        }

        public override void ShowInterstitialAd(TEnum gamePlacement)
        {
            ShowAds(gamePlacement);

            if (Application.isEditor)
            {
                OnShowInterstitialSuccessful();
                return;
            }

#if UNITY_WEBGL
            ShowInterstitial();
#endif
        }

        private void ShowAds(TEnum gamePlacement)
        {
            _startedAd = gamePlacement;
            SetStatusForPlacement(gamePlacement, AdStatusValue.Showed);
        }

        public override void PreloadRewardedAd(TEnum gamePlacement)
        {
            PreloadAds(gamePlacement, LoadRewardedAd);
        }

        public override void PreloadInterstitial(TEnum gamePlacement)
        {
            PreloadAds(gamePlacement, LoadInterstitial);
        }

        private void PreloadAds(TEnum gamePlacement, Action<TEnum, float> loadMethod)
        {
            InitializeAdForPlacement(gamePlacement);
            if (!TryGetStatusForPlacement(gamePlacement, out var status) ||
                status.Value != AdStatusValue.NotReady)
                return;

            if (Application.isEditor)
            {
                SetStatusForPlacement(gamePlacement, AdStatusValue.Ready);
                return;
            }

            loadMethod(gamePlacement, 0f);
        }

        protected void LoadRewardedAd(TEnum gamePlacement, float timeout = 0f)
        {
            if (_loaders.ContainsKey(gamePlacement))
                return;

            var cor = LoadRewardedAdCoroutine(gamePlacement, timeout);
            StartCoroutine(cor);
            _loaders.Add(gamePlacement, cor);
        }

        protected void LoadInterstitial(TEnum gamePlacement, float timeout)
        {
            if (_loaders.ContainsKey(gamePlacement))
                return;

            var cor = LoadInterstitialCoroutine(gamePlacement, timeout);
            StartCoroutine(cor);
            _loaders.Add(gamePlacement, cor);
        }

        private IEnumerator LoadRewardedAdCoroutine(TEnum gamePlacement, float timeout)
        {
            SetStatusForPlacement(gamePlacement, AdStatusValue.InLoading);
            _rewarded.Add(gamePlacement);
            yield return new WaitForSeconds(timeout);
#if UNITY_WEBGL
            LoadRewarded();
#endif

            _loaders.Remove(gamePlacement);
        }

        private IEnumerator LoadInterstitialCoroutine(TEnum gamePlacement, float timeout)
        {
            SetStatusForPlacement(gamePlacement, AdStatusValue.InLoading);
            _interstitial.Add(gamePlacement);
            yield return new WaitForSeconds(timeout);

#if UNITY_WEBGL
            LoadInterstitial();
#endif

            _loaders.Remove(gamePlacement);
        }

        public override bool IsRewardedAdReady(TEnum gamePlacement)
        {
            InitializeAdForPlacement(gamePlacement);

            if (TryGetStatusForPlacement(gamePlacement, out var status))
            {
                if (status.Value != AdStatusValue.Ready)
                {
                    LoadRewardedAd(gamePlacement, Timeout);
                }

                return status.Value == AdStatusValue.Ready;
            }

            return false;
        }

        public override AdStatus.AdStatus InitializeAdForPlacement(TEnum gamePlacement)
        {
            if (!TryGetStatusForPlacement(gamePlacement, out var status))
            {
                status = SetStatusForPlacement(gamePlacement, AdStatusValue.NotReady);
                status.OnStatusChange.AddListener(AnyAdStatusChangedHandler);
            }

            return status;
        }

        private void AnyAdStatusChangedHandler(AdStatusValue status)
        {
            switch (status)
            {
                case AdStatusValue.Showed:
                    AudioListener.volume = 0;
                    AudioListener.pause = true;
                    _adInProgress = true;
                    break;
                case AdStatusValue.SuccessWatched:
                case AdStatusValue.Failed:
                case AdStatusValue.Hidden:
                    AdHandled();
                    break;
            }
        }

        protected virtual void AdHandled()
        {
            _adInProgress = false;
            ResetLastAdTimer();
            AudioListener.volume = 1f;
            AudioListener.pause = false;
        }

        // native js call
        protected abstract void LoadRewarded();
        protected abstract void ShowRewarded();
        protected abstract void LoadInterstitial();
        protected abstract void ShowInterstitial();

        // called from web js
        public virtual void OnLoadingRewardedSuccessful()
        {
            foreach (var rewarded in _rewarded)
            {
                SetStatusForPlacement(rewarded, AdStatusValue.Ready);
            }
        }

        public virtual void OnLoadingRewardedFailed()
        {
            foreach (var rewarded in _rewarded)
            {
                SetStatusForPlacement(rewarded, AdStatusValue.NotReady);
                LoadRewardedAd(rewarded, Timeout);
            }
        }

        public virtual void OnLoadingInterstitialSuccessful()
        {
            foreach (var rewarded in _interstitial)
            {
                SetStatusForPlacement(rewarded, AdStatusValue.Ready);
            }
        }

        public virtual void OnLoadingInterstitialFailed()
        {
            foreach (var interstitial in _interstitial)
            {
                SetStatusForPlacement(interstitial, AdStatusValue.NotReady);
                LoadInterstitial(interstitial, Timeout);
            }
        }

        public virtual void OnShowRewardedSuccessful()
        {
            SetStatusForPlacement(_startedAd, AdStatusValue.SuccessWatched);
            SetStatusForPlacement(_startedAd, AdStatusValue.Ready);
        }

        public virtual void OnShowRewardedFailed()
        {
            SetStatusForPlacement(_startedAd, AdStatusValue.Failed);
            SetStatusForPlacement(_startedAd, AdStatusValue.Ready);
        }

        public virtual void OnShowInterstitialSuccessful()
        {
            SetStatusForPlacement(_startedAd, AdStatusValue.SuccessWatched);
            SetStatusForPlacement(_startedAd, AdStatusValue.Ready);
        }

        public virtual void OnShowInterstitialFailed()
        {
            SetStatusForPlacement(_startedAd, AdStatusValue.Failed);
            SetStatusForPlacement(_startedAd, AdStatusValue.Ready);
        }
    }
}
