using System.Collections.Generic;
using Analytics.Core.Runtime.Interfaces;

namespace Analytics.Core.Runtime
{
	public abstract class AnalyticsManager : IAnalyticManager
	{
		private List<IAnalytics> _analyticsList;

		protected abstract bool CanSendAnalytic { get; }

		public AnalyticsManager(List<IAnalytics> analytics)
		{
			_analyticsList = analytics;
		}

		public void Initialize()
		{
			foreach (var analytics in _analyticsList)
			{
				analytics.Initialize();
			}
		}

		public void SendEvent(IAnalyticData data)
		{
			if (!CanSendAnalytic)
				return;

			foreach (var analytics in _analyticsList)
			{
				if (analytics.IsInitialized() && CanSendCustomEvents(data.Event, analytics.Type))
				{
					var parameters = data.ToDictionary(analytics.Type);
					analytics.SendEvent(data.Event, parameters);
				}
			}
		}

		public void SendEvent(string eventName, Dictionary<string, object> parameters, List<AnalyticType> receivers = null)
		{
			foreach (var analytics in _analyticsList)
			{
				if (receivers != null && !receivers.Contains(analytics.Type))
					continue;

				if (analytics.IsInitialized() && CanSendCustomEvents(eventName, analytics.Type))
				{
					analytics.SendEvent(eventName, parameters);
				}
			}
		}

		public void SendEventRevenue(RevenueData product, Dictionary<string, object> param = null)
		{
			if (!CanSendAnalytic)
				return;

			foreach (var analytics in _analyticsList)
			{
				if (analytics.IsInitialized())
				{
					analytics.SendEventRevenue(product, param);
				}
			}
		}

		public void SendEventsBuffer()
		{
			if (!CanSendAnalytic)
				return;

			foreach (var analytics in _analyticsList)
			{
				if (analytics.IsInitialized())
				{
					analytics.SendEventsBuffer();
				}
			}
		}

		protected abstract bool CanSendCustomEvents(string eventName, AnalyticType analyticType);
	}
}