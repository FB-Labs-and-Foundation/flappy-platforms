using System;
using System.Runtime.InteropServices;
using Plugins.Social.Core.Runtime;
using UnityEngine;

namespace PlayDeck.Runtime.Social
{
	public class PlayDeckSocial : ASocialManager
	{
		public override void Share(string message, string url, Action onSuccess = null, Action onFailed = null)
		{
			onSuccessAction = onSuccess;
			onFailedAction = onFailed;
#if UNITY_WEBGL && !UNITY_EDITOR
			var item = new ShareItem(url, message);
			var result = ToJson(item);
			PlayDeckBridge_PostMessage_CustomShare(result);
#else
			Debug.Log("This function is intended for WebGL builds.");
#endif
		}

		public override void OpenSocialChannel(string link)
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			PlayDeckBridge_PostMessage_OpenTelegramLink(link);
#else
			Debug.Log("This function is intended for WebGL builds.");
#endif
		}

		public override void GetShareLink(string link)
		{
			PlayDeckBridge_PostMessage_GetShareLink(link);
		}

		public override void ReceiveLog(string message)
		{
		}

		private string ToJson(ShareItem parameters)
		{
			return JsonUtility.ToJson(parameters);
			// return JsonConvert.SerializeObject(parameters);
		}

		private void GetShareLinkHandler(string shareLink)
		{
		}

		//Only for PlayDeck
		[Serializable]
		private class ShareItem
		{
			public string url;
			public string message;

			public ShareItem(string url, string message)
			{
				this.url = url;
				this.message = message;
			}
		}

		[DllImport("__Internal")]
		private static extern void PlayDeckBridge_PostMessage_CustomShare(string data);

		[DllImport("__Internal")]
		private static extern void PlayDeckBridge_PostMessage_GetShareLink(string data);

		[DllImport("__Internal")]
		private static extern void PlayDeckBridge_PostMessage_OpenTelegramLink(string data);
	}
}