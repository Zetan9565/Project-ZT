using UnityEngine;
using System.Collections.Generic;

namespace ZetanStudio.ShopSystem
{
    [DisallowMultipleComponent]
    public static class ShopManager
    {
        public static List<TalkerData> Vendors { get; } = new List<TalkerData>();

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            EmptyMonoBehaviour.Singleton.UpdateCallback -= Update;
            EmptyMonoBehaviour.Singleton.UpdateCallback += Update;
        }

        private static void Update()
        {
            RefreshAll(Time.deltaTime);
        }

        public static void RefreshAll(float time)
        {
            using var vendorEnum = Vendors.GetEnumerator();
            while (vendorEnum.MoveNext())
                vendorEnum.Current.shop.TimePass(time);
        }

        #region 消息
        /// <summary>
        /// 商店道具刷新，格式：([数量刷新的商品: <see cref="GoodsData"/>])
        /// </summary>
        public const string vendorGoodsRefresh = "VendorGoodsRefresh";
        #endregion
    }
}