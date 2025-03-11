using System;
using System.Globalization;

namespace Analytics.Core.Runtime
{
	public static class RevenueHelper
	{
		public static string GetRevenueString(this decimal price)
		{
			string priceString;
			try
			{
				var culture = new CultureInfo("en-US");
				priceString = price.ToString(culture);
			}
			catch (Exception)
			{
				priceString = price.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
			}

			return priceString;
		}
	}
}