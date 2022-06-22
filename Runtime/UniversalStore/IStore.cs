using System;
using System.Collections.Generic;

namespace UniversalStore
{
    public interface IStore
    {
        event Action<bool> OnInitialized;

        event Action<PurchaseInfo> OnPurchaseStarted;
        event Action<PurchaseInfo> OnPurchaseSuccess;
        event Action<PurchaseInfo, string> OnPurchaseFailed;

        event Action<bool> OnRestore;

        IDictionary<string, IAPProduct> Products { get; }

        bool IsInitialized { get; }

        void Initialize();

        bool IsPurchased(string id);

        string GetPrice(string id);

        void Buy(string id);

        void RestorePurchases();
        void TryRestorePurchases(Action<bool> callback);

        IStore CreateNewInstance();
    }
}