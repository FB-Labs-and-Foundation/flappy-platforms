namespace InApp.Core.Runtime
{
    public interface IPurchaseHandler
    {
        void OnPurchaseCompleted(string productId, object data = null);
        void ConfirmPurchaseReceiving(string productId);
    }
}