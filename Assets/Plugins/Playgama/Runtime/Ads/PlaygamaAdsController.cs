using System;
using Ads.Core.Runtime;
using Ads.Core.Runtime.AdStatus;
using Playgama;
using Playgama.Modules.Advertisement;
using UnityEngine;

public class PlaygamaAdsController<TEnum> : AdsController<TEnum> where TEnum : Enum
{
	private TEnum _currentPlacement;
	private int _interstitialDelay;
	private bool _adInProgress;
	private bool _initialized;

	public override bool AdInProgress => _adInProgress;

	public bool IsBannersSupported { get; private set; }

	protected override void Initialize(string sdkKey, string playerId, bool ageRestrictedFlag)
	{
#if UNITY_WEBGL
		if (!int.TryParse(PlaygamaSDKProxy.PlaygamaBridgeMinimumDelayBetweenInterstitial(), out _interstitialDelay))
		{
			_interstitialDelay = 60;
			PlaygamaSDKProxy.PlaygamaBridgeSetMinimumDelayBetweenInterstitial(_interstitialDelay.ToString());
		}

		IsBannersSupported = PlaygamaSDKProxy.PlaygamaBridgeIsBannerSupported() == "true";
#else
		_interstitialDelay = 60;
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

		if (Application.isEditor)
		{
			OnRewardedStateChanged(RewardedState.Loading.ToString());
			OnRewardedStateChanged(RewardedState.Opened.ToString());
			OnRewardedStateChanged(RewardedState.Rewarded.ToString());
			OnRewardedStateChanged(RewardedState.Closed.ToString());
		}
		else
		{
			PlaygamaSDKProxy.PlaygamaBridgeShowRewarded();
		}
	}

	public override void ShowInterstitialAd(TEnum placement)
	{
#if !UNITY_EDITOR
            PlaygamaBridgeShowInterstitial();
#else

		if (SecondsFromLastAd > _interstitialDelay)
		{
			OnInterstitialStateChanged(InterstitialState.Loading.ToString());
			OnInterstitialStateChanged(InterstitialState.Opened.ToString());
			OnInterstitialStateChanged(InterstitialState.Closed.ToString());
		}
		else
		{
			OnInterstitialStateChanged(InterstitialState.Failed.ToString());
		}
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
#if !UNITY_EDITOR
            PlaygamaBridgeShowBanner(options.ToJson());
#else
		OnBannerStateChanged(BannerState.Loading.ToString());
		OnBannerStateChanged(BannerState.Shown.ToString());
#endif
	}

	public override void HideBanner(TEnum placement)
	{
#if !UNITY_EDITOR
            PlaygamaBridgeHideBanner();
#else
		OnBannerStateChanged(BannerState.Hidden.ToString());
#endif
	}

	public override void DestroyBanner(TEnum placement)
	{
	}

	#region Called From js

	private void OnBannerStateChanged(string value)
	{
		AdHandled();
		if (Enum.TryParse<BannerState>(value, true, out var state))
		{
			Debug.Log($"OnBannerStateChanged: {state}");
		}
	}

	private void OnRewardedStateChanged(string value)
	{
		AdHandled();

		if (Enum.TryParse<RewardedState>(value, true, out var state))
		{
			Debug.Log($"OnRewardedStateChanged: {state}");
		}
	}

	private void OnInterstitialStateChanged(string value)
	{
		AdHandled();

		if (Enum.TryParse<InterstitialState>(value, true, out var state))
		{
			Debug.Log($"OnInterstitialStateChanged: {state}");
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