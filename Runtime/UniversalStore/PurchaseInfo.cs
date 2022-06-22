using UnityEngine.Purchasing;

namespace UniStore
{
    public struct PurchaseInfo
    {
        public string ProductId;

        public string Price;
        public string Currency;

        public ProductType Type;
    }
}