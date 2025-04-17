using System.Runtime.InteropServices;

public partial class PlaygamaSDKProxy
{
    [DllImport("__Internal")]
    public static extern string PlaygamaBridgeGetInterstitialState();

    [DllImport("__Internal")]
    public static extern string PlaygamaBridgeMinimumDelayBetweenInterstitial();

    [DllImport("__Internal")]
    public static extern void PlaygamaBridgeSetMinimumDelayBetweenInterstitial(string options);

    [DllImport("__Internal")]
    public static extern void PlaygamaBridgeShowInterstitial();

    [DllImport("__Internal")]
    public static extern void PlaygamaBridgeShowRewarded();

    [DllImport("__Internal")]
    public static extern string PlaygamaBridgeIsBannerSupported();

    [DllImport("__Internal")]
    public static extern void PlaygamaBridgeShowBanner(string options);

    [DllImport("__Internal")]
    public static extern void PlaygamaBridgeHideBanner();

    [DllImport("__Internal")]
    public static extern void PlaygamaBridgeCheckAdBlock();
}
