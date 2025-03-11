namespace Analytics.Core.Runtime
{
	public struct RevenueData
	{
		public readonly string TransactionId;
		public readonly decimal DecimalPrice;
		public readonly string Currency;
		public readonly string StoreId;
		public readonly string ContentType;

		public RevenueData(string transactionId, decimal decimalPrice, string currency, string storeId, string contentType)
		{
			TransactionId = transactionId;
			DecimalPrice = decimalPrice;
			Currency = currency;
			StoreId = storeId;
			ContentType = contentType;
		}
	}
}