using System;
using System.Runtime.InteropServices;
using Plugins.Social.Core.Runtime;
using UnityEngine;

namespace Plugins.Social.Telegram.Rutime
{
	public class TelegramSocialManager : ASocialManager
	{
		public override void Share(string message, string url, Action onSuccess = null, Action onFailed = null)
		{
			onSuccessAction = onSuccess;
#if UNITY_WEBGL && !UNITY_EDITOR
			ShareOnTelegram(message,url);
#else
			Debug.Log("This function is intended for WebGL builds.");
#endif
		}

		public override void OpenSocialChannel(string channel)
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			OpenTelegramChannel(channel);
#else
			Debug.Log("This function is intended for WebGL builds.");
#endif
		}

		// This method will be called from JS
		protected override void OnActionComplete()
		{
			Debug.Log("On Share Complete");
			base.OnActionComplete();
		}

		// This method will be called from JS
		protected override void OnActionFailed()
		{
			Debug.Log("On Share Failed");
			base.OnActionFailed();
		}

		public override void GetShareLink(string link)
		{
			throw new NotImplementedException();
		}

		// This method will be called from JS
		public override void ReceiveLog(string message)
		{
			Debug.Log("TelegramShare: " + message);
		}

		[DllImport("__Internal")]
		private static extern void ShareOnTelegram(string message, string url);

		[DllImport("__Internal")]
		private static extern void OpenTelegramChannel(string username);
	}
}