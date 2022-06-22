#if SAMSUNG
using System;
using System.Collections.Generic;
using System.Linq;
using Samsung;
using UnityEngine;

namespace Monetization
{
    public class SamsungStore : BaseStore
    {
        public Action<ProductInfoList> OnGetProductsDetailsListener;
        public Action<PurchasedInfo> OnStartPaymentListener;
        public Action<ConsumedList> OnConsumePurchasedItemListener;
        public Action<OwnedProductList> OnGetOwnedListListener;

        public override bool IsInitialized => true;

        private readonly HashSet<string> _purchased;

        private readonly Dictionary<string, ProductVo> _productInfos;

        private AndroidJavaObject _iapInstance;

        private string _savedPassthroughParam = "";

        public SamsungStore(IEnumerable<IAPProduct> products, IValidator validator = null) :
            base(products, validator)
        {
            _purchased = new HashSet<string>();
            _productInfos = new Dictionary<string, ProductVo>();
        }

        public override void Initialize()
        {
            using (var cls = new AndroidJavaClass("com.samsung.android.sdk.iap.lib.activity.SamsungIAPFragment"))
            {
                //Initialize IAP
                cls.CallStatic("init", ToString());

                _iapInstance = cls.CallStatic<AndroidJavaObject>("getInstance");

                SetOperationMode
                (
#if DEBUG
                    OperationMode.OPERATION_MODE_TEST
#else
                    OperationMode.OPERATION_MODE_PRODUCTION
#endif
                );

                Initialized(_iapInstance != null);

                var ids = Products.Aggregate(string.Empty, (current, pair) => current + pair.Key + ", ");
                GetProductsDetails
                (
                    ids,
                    productInfoList =>
                    {
                        foreach (var result in productInfoList.results)
                        {
                            _productInfos.Add(result.mItemId, result);
                        }
                    }
                );
            }
        }

        public override bool IsPurchased(string id)
        {
            return _purchased.Contains(id);
        }

        public override string GetPrice(string id)
        {
            return !_productInfos.TryGetValue(id, out var info) ? "$0.01 (fake)" : info.mItemPrice;
        }

        protected override void BuyProcess(string id)
        {
            StartPayment(id, "", null);
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

        #region IAP Functions

        public void SetOperationMode(OperationMode mode)
        {
            if (_iapInstance != null)
            {
                _iapInstance.Call("setOperationMode", mode.ToString());
            }
            else
            {
#if DEBUG
                UnityEngine.Debug.LogError("<b>[SamsungStore]</b> Android Context not initialized correctly.");
#endif
            }
        }

        public void GetProductsDetails(string itemIDs, Action<ProductInfoList> listener)
        {
            OnGetProductsDetailsListener = listener;

            if (_iapInstance != null)
            {
                _iapInstance.Call("getProductDetails", itemIDs);
            }
            else
            {
#if DEBUG
                UnityEngine.Debug.LogError("<b>[SamsungStore]</b> Android Context not initialized correctly.");
#endif
            }
        }

//         public void GetOwnedList(ItemType itemType, Action<OwnedProductList> listener)
//         {
//             OnGetOwnedListListener = listener;
//
//             if (_iapInstance != null)
//             {
//                 _iapInstance.Call("getOwnedList", itemType.ToString());
//             }
//             else
//             {
// #if DEBUG
//                 UnityEngine.Debug.LogError("<b>[SamsungStore]</b> Android Context not initialized correctly.");
// #endif
//             }
//         }

        public void StartPayment(string itemID, string passThroughParam, Action<PurchasedInfo> listener)
        {
            _savedPassthroughParam = passThroughParam;

            OnStartPaymentListener = listener;

            if (_iapInstance != null)
            {
                _iapInstance.Call("startPayment", itemID, passThroughParam);
            }
            else
            {
#if DEBUG
                UnityEngine.Debug.LogError("<b>[SamsungStore]</b> Android Context not initialized correctly.");
#endif
            }
        }

        public void ConsumePurchasedItems(string purchaseIDs, Action<ConsumedList> listener)
        {
            Debug.Log("ConsumePurchasedItems : !");
            OnConsumePurchasedItemListener = listener;

            if (_iapInstance != null)
            {
                _iapInstance.Call("consumePurchasedItems", purchaseIDs);
            }
            else
            {
#if DEBUG
                UnityEngine.Debug.LogError("<b>[SamsungStore]</b> Android Context not initialized correctly.");
#endif
            }
        }

        #endregion

        #region CALLBACKS

        public void OnGetProductsDetails(string resultJSON)
        {
            var productList = JsonUtility.FromJson<ProductInfoList>(resultJSON);
#if DEBUG
            UnityEngine.Debug.Log($"<b>[SamsungStore]</b> OnGetProductsDetails : {resultJSON}");
            UnityEngine.Debug.Log($"<b>[SamsungStore]</b> OnGetProductsDetails cnt: {productList.results.Count}");

            for (var i = 0; i < productList.results.Count; ++i)
            {
                UnityEngine.Debug.Log(
                    $"<b>[SamsungStore]</b> onGetProductsDetails: {productList.results[i].mItemName}");
            }
#endif
            OnGetProductsDetailsListener?.Invoke(productList);
        }

        public void OnGetOwnedProducts(string resultJSON)
        {
            var ownedList = JsonUtility.FromJson<OwnedProductList>(resultJSON);

#if DEBUG
            UnityEngine.Debug.Log($"<b>[SamsungStore]</b> onGetOwnedProducts cnt: {ownedList.results.Count}");

            foreach (var productVo in ownedList.results)
            {
                Debug.Log("onGetOwnedProducts: " + productVo.mItemName);
            }
#endif
            OnGetOwnedListListener?.Invoke(ownedList);
        }

        public void OnConsumePurchasedItems(string resultJSON)
        {
            var consumedList = JsonUtility.FromJson<ConsumedList>(resultJSON);

#if DEBUG
            UnityEngine.Debug.Log($"<b>[SamsungStore]</b> OnConsumePurchasedItems: {resultJSON}");
            UnityEngine.Debug.Log($"<b>[SamsungStore]</b> OnConsumePurchasedItems cnt: {consumedList.results.Count}");

            foreach (var consumeResult in consumedList.results)
            {
                Debug.Log("<b>[SamsungStore]</b> OnConsumePurchasedItems: " + consumeResult.mPurchaseId);
            }
#endif
            OnConsumePurchasedItemListener?.Invoke(consumedList);
        }

        public void OnPayment(string resultJSON)
        {
            var purchasedInfo = JsonUtility.FromJson<PurchasedInfo>(resultJSON);

            _purchased.Add(purchasedInfo.results.mItemId);
#if DEBUG
            if (purchasedInfo.results.mPassThroughParam != _savedPassthroughParam)
            {
                UnityEngine.Debug.Log("<b>[SamsungStore]</b> PassThroughParam is different!!!");
            }
#endif
            OnStartPaymentListener?.Invoke(purchasedInfo);
        }

        #endregion
    }
}
#endif