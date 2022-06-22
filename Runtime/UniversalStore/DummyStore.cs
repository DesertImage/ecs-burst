using System;
using System.Collections.Generic;

namespace UniversalStore
{
    public class DummyStore : BaseStore
    {
        public override bool IsInitialized => true;

        private readonly HashSet<string> _purchased;

        public DummyStore(IEnumerable<IAPProduct> products, IValidator validator = null) : base(products, validator)
        {
            _purchased = new HashSet<string>();
        }

        public override void Initialize()
        {
        }

        public override bool IsPurchased(string id)
        {
            return _purchased.Contains(id);
        }

        public override string GetPrice(string id)
        {
            return "$0.01 (fake)";
        }

        protected override void BuyProcess(string id)
        {
            _purchased.Add(id);

            PurchaseSuccess
            (
                new PurchaseInfo
                {
                    ProductId = id,
                    Price = GetPrice(id)
                }
            );
        }

        #region RESTORE

        public override void RestorePurchases()
        {
            TryRestorePurchases(Restored);
        }

        public override void TryRestorePurchases(Action<bool> callback)
        {
            callback?.Invoke(true);
        }

        #endregion

        public override IStore CreateNewInstance()
        {
            return new DummyStore(Products?.Values, Validator);
        }
    }
}