using System.Collections.Generic;
using System.Runtime.InteropServices;
using Analytics.Core.Runtime;
using Analytics.Core.Runtime.Interfaces;
using UnityEngine.Device;

namespace Plugins.Analytics
{
	public class AnalyticsFirebaseWebgl : IAnalytics
	{
		public AnalyticType Type => AnalyticType.FirebaseWebgl;

		public void Initialize()
		{
		}

		public bool IsInitialized()
		{
			return true;
		}

		public void SendEvent(string eventName, Dictionary<string, object> parameters = null)
		{
			if (parameters is null)
			{
				if (!Application.isEditor)
					LogEvent(eventName);
			}
			else
			{
				var json = ToJson(parameters);
				if (!Application.isEditor)
					LogEventParameter(eventName, json);
			}
		}

		public void SendEventRevenue(RevenueData product, Dictionary<string, object> parameters = null)
		{
			parameters ??= new Dictionary<string, object>();

			var localizedString = product.DecimalPrice.ToString();
			if (!double.TryParse(localizedString, out double price))
			{
				price = 1;
			}

			parameters.Add("value", price);
			parameters.Add("currency", product.Currency);

			SendEvent("in_app_purchase", parameters);
		}

		public void SendEventsBuffer()
		{
		}

		private string ToJson(Dictionary<string, object> parameters)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
			//var entries = parameters.Select(d =>
			//    $"\"{d.Key}\": {string.Join(",", d.Value)}");
			//return "{" + string.Join(",", entries) + "}";
		}

		/// <summary>
		/// Log an event without parameter
		/// </summary>
		/// <param name="eventName">Name of the event</param>
		[DllImport("__Internal")]
		public static extern void LogEvent(string eventName);

		/// <summary>
		/// Log an event with parameter
		/// </summary>
		/// <param name="eventName">Name of the event</param>
		/// <param name="eventParam">JSON-formatted string of parameter.
		/// ex : {"name":"MonsterName", "lives":"3"}
		/// </param>
		[DllImport("__Internal")]
		public static extern void LogEventParameter(string eventName, string eventParam);
	}
}