#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif
namespace PlayDeck.Runtime.Ads
{
	public class PlayDeckAdsProxy
	{
#if UNITY_WEBGL
		[DllImport("__Internal")]
		public static extern void PlayDeckBridge_PostMessage_ShowAd();

#endif
	}
}