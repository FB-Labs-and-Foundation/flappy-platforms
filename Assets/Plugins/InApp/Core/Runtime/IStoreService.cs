namespace InApp.Core.Runtime
{
	public interface IStoreService
	{
		public bool IsPurchaseInProcess { get; }
		public bool IsInitialized();
		public void Purchase(string iapID);
		public void Consume(string productId);
		public decimal GetCost(string productId);
		public void ConfirmPurchaseReceiving(string productId);
		public void Cancel();
	}
}