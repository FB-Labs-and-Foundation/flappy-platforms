using System.Collections.Generic;
using Analytics.Core.Runtime.Interfaces;

namespace Analytics.Core.Runtime
{
	public class DefaultAnalyticsManager: AnalyticsManager
	{
		public DefaultAnalyticsManager(List<IAnalytics> analytics) : base(analytics)
		{
		}

		protected override bool CanSendAnalytic => true;

		protected override bool CanSendCustomEvents(string eventName, AnalyticType analyticType)
		{
			return true;
		}
	}
}