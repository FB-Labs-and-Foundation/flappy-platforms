using UnityEngine;

namespace Plugins.Social.Core.Runtime
{
	public abstract class ASocialManager : MonoBehaviour
	{
		protected System.Action onSuccessAction;
		protected System.Action onFailedAction;

		public bool IsAvailable { get; }

		public abstract void Share(string message,
			string url,
			System.Action onSuccess = null,
			System.Action onFailed = null);

		public abstract void OpenSocialChannel(string channelName);

		protected virtual void OnActionComplete()
		{
			onSuccessAction?.Invoke();
		}

		protected virtual void OnActionFailed()
		{
			onFailedAction?.Invoke();
		}

		public abstract void GetShareLink(string link);

		public abstract void ReceiveLog(string message);
	}
}