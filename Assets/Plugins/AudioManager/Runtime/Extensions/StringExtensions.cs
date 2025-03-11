using System;
using System.Globalization;

namespace AudioManager.Runtime.Extensions
{
	public static class StringExtensions
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

		public static bool IsNullOrEmpty(this string str)
		{
			return str == null || str.Equals(string.Empty);
		}
	}
}