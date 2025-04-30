using System;
using UnityEngine;

namespace Plugins.Social.Core.Runtime
{
	public class EmptySocialManager : ASocialManager
	{

		public override void Share(string message, string url, Action onSuccess = null, Action onFailed = null)
		{
			Debug.Log($"Empty Social Manager \nmessage: {message}\n url: {url}");
			onSuccessAction = onSuccess;
			onFailedAction = onFailed;
		}

		public override void OpenSocialChannel(string channelName)
		{
			Debug.Log($"Empty Social Manager \nchannelName: {channelName}");

		}

		public override void GetShareLink(string link)
		{
		}

		public override void ReceiveLog(string message)
		{
			Debug.Log(message);
		}
	}
}