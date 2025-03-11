using System.Collections.Generic;

namespace Analytics.Core.Runtime.Interfaces
{
	public interface IAnalytics
	{
		public AnalyticType Type { get; }

		void Initialize();
		bool IsInitialized();
		void SendEvent(string eventName, Dictionary<string, object> param = null);
		void SendEventRevenue(RevenueData product, Dictionary<string, object> param = null);
		void SendEventsBuffer();
	}
}