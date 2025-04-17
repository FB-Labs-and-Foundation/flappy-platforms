using System.Runtime.InteropServices;

namespace Playgama
{
	public partial class PlaygamaSDKProxy
	{
		[DllImport("__Internal")]
		public static extern string PlaygamaBridgeGetPlatformId();

		[DllImport("__Internal")]
		public static extern string PlaygamaBridgeGetPlatformLanguage();

		[DllImport("__Internal")]
		public static extern string PlaygamaBridgeGetPlatformPayload();

		[DllImport("__Internal")]
		public static extern string PlaygamaBridgeGetPlatformTld();

		[DllImport("__Internal")]
		public static extern void PlaygamaBridgeSendMessageToPlatform(string message);

		[DllImport("__Internal")]
		public static extern string PlaygamaBridgeGetServerTime();
	}
}