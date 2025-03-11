using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Analytics.Core.Runtime;
using Analytics.Core.Runtime.Interfaces;
using UnityEngine;

namespace PlayDeck.Runtime.Analytics
{
	public class PlayDeckAnalytics : IAnalytics
	{
		public AnalyticType Type => AnalyticType.PlayDeck;

		public void Initialize()
		{
		}

		public bool IsInitialized()
		{
			return true;
		}

		public void SendEvent(string eventName, Dictionary<string, object> param = null)
		{
			var analyticsEvent = new AnalyticsEvent
			{
				type = eventName,
				event_properties = param
			};
			var result = ToJson(analyticsEvent);
			if (!Application.isEditor)
				PlayDeckBridge_PostMessage_SendAnalytics(result);
		}

		public void SendEventRevenue(RevenueData product, Dictionary<string, object> param = null)
		{
			param ??= new Dictionary<string, object>();

			var localizedString = product.DecimalPrice.ToString();
			if (!double.TryParse(localizedString, out double price))
			{
				price = 1;
			}

			param.Add("value", price);
			param.Add("currency", product.Currency);

			SendEvent("in_app_purchase", param);
		}

		public void SendEventsBuffer()
		{
		}

		private string ToJson(AnalyticsEvent parameters)
		{
			return JsonUtility.ToJson(parameters);
			//return JsonConvert.SerializeObject(parameters);
		}

		//For PlayDeck only
		[Serializable]
		private class AnalyticsEvent
		{
			public string type;
			public Dictionary<string, object> event_properties;
		}

		[DllImport("__Internal")]
		private static extern void PlayDeckBridge_PostMessage_SendAnalytics(string data);

		[DllImport("__Internal")]
		private static extern void PlayDeckBridge_PostMessage_SendAnalyticsNewSession();

		[DllImport("__Internal")]
		private static extern void PlayDeckBridge_PostMessage_GameEnd();
	}
}