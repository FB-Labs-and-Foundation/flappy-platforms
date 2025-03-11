using System;
using UnityEngine.Events;

namespace Ads.Core.Runtime
{
	public interface IAdsController<TEnum> where TEnum : Enum
	{
		bool IsInitialized();
		float SecondsFromLastAd { get; }
		bool AdInProgress { get; }
		void Initialize(GamePlacements<TEnum> placements, string sdkKey, string playerId, bool ageRestrictedFlag);
		AdStatus.AdStatus InitializeAdForPlacement(TEnum placement);
		bool TryGetStatusForPlacement(TEnum gamePlacement, out AdStatus.AdStatus status);
		void ShowRewardedAd(TEnum placement);
		void ShowInterstitialAd(TEnum placement);
		void PreloadRewardedAd(TEnum placement);
		void PreloadInterstitial(TEnum placement);
		bool IsRewardedAdReady(TEnum placement);
		void ResetLastAdTimer();
		void AddAdSuccessWatchedListener(TEnum gamePlacement, UnityAction callback);
		void RemoveAdSuccessWatchedListener(TEnum gamePlacement, UnityAction callback);
		void CreateBanner(IBannerData data);
		void ShowBanner(TEnum placement);
		void HideBanner(TEnum placement);
		void DestroyBanner(TEnum placement);
	}
}