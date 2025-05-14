namespace InApp.Core.Runtime
{
	public interface IStoreService
	{
		public bool IsInitialized();
		public void Purchase(string productId);
		public decimal GetCost(string productId);
		public string GetLocalizedCost(string productId);
		public string GetCurrency(string productId);
		public void ConfirmPurchaseReceiving(string productId);
		void ProcessPendingPurchases();
	}
}