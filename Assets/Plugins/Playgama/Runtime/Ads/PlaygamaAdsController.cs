using System;
using System.Collections.Generic;
using Ads.Core.Runtime;
using Ads.Core.Runtime.AdStatus;
using Playgama;
using Playgama.Modules.Advertisement;
using Plugins.Playgama.Runtime.Ads;
using UnityEngine;

public class PlaygamaAdsController<TEnum> : AdsController<TEnum> where TEnum : Enum
{
	private TEnum _currentPlacement;
	private int _interstitialDelay;
	private bool _adInProgress;
	private bool _initialized;

	public override bool AdInProgress => _adInProgress;

	public bool IsBannersSupported { get; private set; }

	protected Dictionary<string, object> _bannerOptions = new Dictionary<string, object>();

	protected override void Initialize(string sdkKey, string playerId, bool ageRestrictedFlag)
	{
#if !UNITY_EDITOR

		IsBannersSupported = PlaygamaSDKProxy.PlaygamaBridgeIsBannerSupported() == "true";
#else
		IsBannersSupported = true;
#endif


		_initialized = true;
	}

	public override bool IsInitialized() => _initialized;

	public override AdStatus InitializeAdForPlacement(TEnum placement)
	{
		return SetStatusForPlacement(placement, AdStatusValue.Ready);
	}

	public override void ShowRewardedAd(TEnum placement)
	{
		StartAdHandler(placement);

		#if !UNITY_EDITOR
		PlaygamaSDKProxy.PlaygamaBridgeShowRewarded();
		#else
		OnRewardedStateChanged(PlaygamaAdStatus.Rewarded.ToString());
		#endif
	}

	public override void ShowInterstitialAd(TEnum placement)
	{
		StartAdHandler(placement);
#if !UNITY_EDITOR
            PlaygamaSDKProxy.PlaygamaBridgeShowInterstitial();
#else

		OnInterstitialStateChanged(PlaygamaAdStatus.Shown.ToString());

#endif
	}

	public override void PreloadRewardedAd(TEnum placement)
	{
	}

	public override void PreloadInterstitial(TEnum placement)
	{
	}

	public override bool IsRewardedAdReady(TEnum placement)
	{
		return true;
	}

	public override void CreateBanner(IBannerData data)
	{
	}

	public override void ShowBanner(TEnum placement)
	{
		StartAdHandler(placement);
#if !UNITY_EDITOR
		var options = JsonUtility.ToJson(_bannerOptions);
		PlaygamaSDKProxy.PlaygamaBridgeShowBanner(options);
#else
		OnBannerStateChanged(PlaygamaAdStatus.Shown.ToString());
#endif
	}

	public override void HideBanner(TEnum placement)
	{
#if !UNITY_EDITOR
            PlaygamaSDKProxy.PlaygamaBridgeHideBanner();
#else
		if (this.TryGetStatusForPlacement(placement, out var status))
		{
			status.Set(AdStatusValue.Hidden);
		}
#endif
	}

	public override void DestroyBanner(TEnum placement)
	{
	}

	#region Called From js

	private void OnBannerStateChanged(string value)
	{
		AdHandled();
		if (Enum.TryParse<PlaygamaAdStatus>(value, true, out var state))
		{
			Debug.Log($"OnBannerStateChanged: {state}");
			var newState = AdStatusConverter.ConvertStatus(state);
			if (this.TryGetStatusForPlacement(_currentPlacement, out var status))
			{
				status.Set(newState);
			}
		}
	}

	private void OnRewardedStateChanged(string value)
	{
		AdHandled();

		if (Enum.TryParse<PlaygamaAdStatus>(value, true, out var state))
		{
			Debug.Log($"OnRewardedStateChanged: {state}");
			var newState = AdStatusConverter.ConvertStatus(state);
			if (this.TryGetStatusForPlacement(_currentPlacement, out var status))
			{
				status.Set(newState);
			}
		}
	}

	private void OnInterstitialStateChanged(string value)
	{
		AdHandled();

		if (Enum.TryParse<PlaygamaAdStatus>(value, true, out var state))
		{
			Debug.Log($"OnInterstitialStateChanged: {state}");
			var newState = AdStatusConverter.ConvertStatus(state);
			if (this.TryGetStatusForPlacement(_currentPlacement, out var status))
			{
				status.Set(newState);
			}
		}
	}

	#endregion

	protected virtual void StartAdHandler(TEnum placement)
	{
		AudioListener.volume = 0;
		AudioListener.pause = true;
		_adInProgress = true;
		_currentPlacement = placement;
	}

	protected virtual void AdHandled()
	{
		_adInProgress = false;
		ResetLastAdTimer();
		AudioListener.volume = 1f;
		AudioListener.pause = false;
	}
}