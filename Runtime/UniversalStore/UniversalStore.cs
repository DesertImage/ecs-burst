using System;
using System.Collections.Generic;

namespace UniStore
{
    public class UniversalStore : IStore
    {
        public event Action<bool> OnInitialized;
        public event Action<PurchaseInfo> OnPurchaseStarted;
        public event Action<PurchaseInfo> OnPurchaseSuccess;
        public event Action<PurchaseInfo, string> OnPurchaseFailed;
        public event Action<bool> OnRestore;

        public IDictionary<string, IAPProduct> Products => _store?.Products;
        public bool IsInitialized => _store?.IsInitialized ?? false;

        private readonly IStore _store;

        public UniversalStore(IEnumerable<IAPProduct> products, string validationUrl = "")
        {
            IValidator validator = string.IsNullOrEmpty(validationUrl) ? null : new BaseValidator(validationUrl);

#if DUMMY_STORE
            _store = new DummyStore(products);
#else
#if HUAWEI
            _store = new HuaweiStore(products);
#elif SAMSUNG
            _store = new SamsungStore(products);
#elif UNITY_ANDROID
            validator = string.IsNullOrEmpty(validationUrl) ? null : new AndroidValidator(validationUrl);
            _store = new UnityPurchasingStore(products, validator);
#elif UNITY_WSA || UNITY_WSA_10_0
            _store = new WindowsStore(products, validator);
#else
            _store = new UnityPurchasingStore(products, validator);
#endif
#endif
            _store.OnInitialized += OnInitialized;

            _store.OnPurchaseStarted += OnPurchaseStarted;
            _store.OnPurchaseSuccess += OnPurchaseSuccess;
            _store.OnPurchaseFailed += OnPurchaseFailed;

            _store.OnRestore += OnRestore;
        }

        public void Initialize()
        {
            _store.Initialize();
        }

        public bool IsPurchased(string id)
        {
            return _store.IsPurchased(id);
        }

        public string GetPrice(string id)
        {
            return _store.GetPrice(id);
        }

        public void Buy(string id)
        {
            _store.Buy(id);
        }

        public void RestorePurchases()
        {
            _store.RestorePurchases();
        }

        public void TryRestorePurchases(Action<bool> callback)
        {
            _store.TryRestorePurchases(callback);
        }

        public IStore CreateNewInstance()
        {
            return _store.CreateNewInstance();
        }
    }
}