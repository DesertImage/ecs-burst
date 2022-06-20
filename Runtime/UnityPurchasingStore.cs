#if !HUAWEI && !SAMSUNG
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Monetization
{
    public class UnityPurchasingStore : BaseStore, IStoreListener
    {
        public override bool IsInitialized =>
            !_initializationFailed && _storeController != null && _extensionProvider != null;

        private bool _initializationFailed;

        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;

        private readonly HashSet<string> _purchased;

        public UnityPurchasingStore(IEnumerable<IAPProduct> products, IValidator validator = null) : base(products,
            validator)
        {
            _purchased = new HashSet<string>();
        }

        public override void Initialize()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (var pair in Products)
            {
                var id = pair.Key;

                var catalog = ProductCatalog.LoadDefaultCatalog();

                var product = catalog.allProducts.First(x => x.id == id);
                builder.AddProduct(id, product.type);
            }

            SetupBuilder(builder);

            UnityPurchasing.Initialize(this, builder);
        }

        protected virtual void SetupBuilder(ConfigurationBuilder builder)
        {
        }

        public override bool IsPurchased(string id)
        {
            return _purchased.Contains(id);
        }

        public override string GetPrice(string id)
        {
            try
            {
                return _storeController.products.WithID(id).metadata.localizedPriceString;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.LogError(ex);
#endif
            }

            return string.Empty;
        }

        protected override void BuyProcess(string id)
        {
            if (!IsInitialized) return;

            _storeController.InitiatePurchase(id);
        }

        #region RESTORE

        public override void RestorePurchases()
        {
            TryRestorePurchases(Restored);
        }

        public override void TryRestorePurchases(Action<bool> callback)
        {
            var restoreResult = false;

            if (!IsInitialized)
            {
                callback?.Invoke(false);

                return;
            }

#if UNITY_STANDALONE
            callback?.Invoke(true);
#else
#if UNITY_ANDROID
            _extensionProvider.GetExtension<IGooglePlayStoreExtensions>().RestoreTransactions(result =>
#elif UNITY_IOS
            _extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(result =>
#endif
            {
                restoreResult = result;
                callback?.Invoke(restoreResult);
            });
#endif
        }
        #endregion

        public override IStore CreateNewInstance()
        {
            return new UnityPurchasingStore(Products?.Values, Validator);
        }

        private void ApplyPurchase(PurchaseEventArgs purchaseEvent)
        {
            var id = purchaseEvent.purchasedProduct.definition.id;

            var productType = purchaseEvent.purchasedProduct.definition.type;
            if (productType == ProductType.NonConsumable && !_purchased.Contains(id))
            {
                _purchased.Add(id);
            }
        }

        #region CALLBACKS

        void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensionProvider)
        {
            _storeController = controller;
            _extensionProvider = extensionProvider;

            Initialized(true);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
#if DEBUG
            UnityEngine.Debug.LogError("<b>[UnityPurchasingStore]</b> initialization failed:\n{error}");
#endif
            _initializationFailed = true;

            Initialized(false);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var id = purchaseEvent.purchasedProduct.definition.id;

            if (!purchaseEvent.purchasedProduct.hasReceipt) return PurchaseProcessingResult.Complete;

            void SuccessPurchase()
            {
            }

            if (Validator != null)
            {
                var receiptPayload = string.Empty;
#if UNITY_IOS
                    var receipt = JsonConvert.DeserializeObject<Receipt>(purchaseEvent.purchasedProduct.receipt);
                    receiptPayload = receipt?.Payload;
#endif
                ValidationProcess(receiptPayload, id, result =>
                {
#if DEBUG
                    UnityEngine.Debug.LogError("<b>[UnityPurchasingStore]</b> VALIDATION RESULT HANDLED");
#endif

                    ApplyPurchase(purchaseEvent);

                    var metadata = purchaseEvent.purchasedProduct.metadata;

                    PurchaseSuccess
                    (
                        new PurchaseInfo
                        {
                            ProductId = id,
                            Price = metadata.localizedPrice.ToString(CultureInfo.InvariantCulture),
                            Currency = metadata.isoCurrencyCode
                        }
                    );
                });
            }
            else
            {
                SuccessPurchase();
            }

            return PurchaseProcessingResult.Complete;
        }

        void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            var id = product.definition.id;

            PurchaseFailed
            (
                new PurchaseInfo
                {
                    ProductId = id,
                    Price = product.metadata.localizedPrice.ToString(CultureInfo.InvariantCulture),
                    Currency = product.metadata.isoCurrencyCode
                },
                failureReason.ToString()
            );
        }

        #endregion
    }
}
#endif