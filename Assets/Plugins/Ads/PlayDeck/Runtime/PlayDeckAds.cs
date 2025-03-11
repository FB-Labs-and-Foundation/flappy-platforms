using System;
using Ads.Core.Runtime;
using Ads.Core.Runtime.AdStatus;
using UnityEngine;

namespace PlayDeck.Runtime.Ads
{
	public class PlayDeckAds<TEnum> : AdsController<TEnum> where TEnum : Enum
	{
		private bool _adInProgress;
		public override bool AdInProgress => _adInProgress;

		private TEnum _currentPlacement;

		protected override void Initialize(string sdkKey, string playerId, bool ageRestrictedFlag)
		{
		}

		public override bool IsInitialized()
		{
			return true;
		}

		public override AdStatus InitializeAdForPlacement(TEnum placement)
		{
			return SetStatusForPlacement(placement, AdStatusValue.Ready);
		}

		public override bool IsRewardedAdReady(TEnum placement)
		{
			return true;
		}

		public override void PreloadRewardedAd(TEnum placement)
		{
		}

		public override void ShowRewardedAd(TEnum placement)
		{
			AudioListener.volume = 0;
			AudioListener.pause = true;
			_adInProgress = true;
			_currentPlacement = placement;
#if UNITY_WEBGL
			if (Application.isEditor)
			{
				OnRewardedVideoClosed();
				OnRewardedVideoWatched();
			}
			else
			{
				PlayDeckAdsProxy.PlayDeckBridge_PostMessage_ShowAd();
			}

#endif
		}

		private void AdHandled()
		{
			_adInProgress = false;
			ResetLastAdTimer();
			AudioListener.volume = 1f;
			AudioListener.pause = false;
		}

		#region Called from JS

		public void OnRewardedVideoClosed()
		{
			AdHandled();

			if (TryGetStatusForPlacement(_currentPlacement, out var rewardStatus))
			{
				rewardStatus.Set(AdStatusValue.Hidden);
				rewardStatus.Set(AdStatusValue.Ready);
			}
		}

		public void OnRewardedVideoWatched()
		{
			if (TryGetStatusForPlacement(_currentPlacement, out var rewardStatus))
			{
				rewardStatus.Set(AdStatusValue.SuccessWatched);
				rewardStatus.Set(AdStatusValue.Ready);
			}
		}

		public void OnRewardedVideoFailed()
		{
			AdHandled();
			if (TryGetStatusForPlacement(_currentPlacement, out var rewardStatus))
			{
				rewardStatus.Set(AdStatusValue.Failed);
				rewardStatus.Set(AdStatusValue.Ready);
			}
		}

		#endregion

		#region Banners

		public override void CreateBanner(IBannerData data)
		{
			throw new NotImplementedException();
		}

		public override void ShowBanner(TEnum placement)
		{
			throw new NotImplementedException();
		}

		public override void HideBanner(TEnum placement)
		{
			throw new NotImplementedException();
		}

		public override void DestroyBanner(TEnum placement)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Interstitials

		public override void PreloadInterstitial(TEnum placement)
		{
			throw new NotImplementedException();
		}

		public override void ShowInterstitialAd(TEnum placement)
		{
			throw new NotImplementedException();
		}

		#endregion


	}
}