using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class ShopManager : SingletonMonoBehaviour<ShopManager>
{
    public static List<TalkerData> Vendors { get; } = new List<TalkerData>();


    private void Update()
    {
        RefreshAll(Time.deltaTime);
    }

    public void RefreshAll(float time)
    {
        /*System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();*/
        using (var vendorEnum = Vendors.GetEnumerator())
            while (vendorEnum.MoveNext())
                vendorEnum.Current.shop.TimePass(time);
        /*stopwatch.Stop();
        Debug.Log(stopwatch.Elapsed.TotalMilliseconds);*/
    }

    #region 消息
    /// <summary>
    /// 商店道具刷新，格式：([数量刷新的商品: <see cref="GoodsData"/>])
    /// </summary>
    public const string VendorGoodsRefresh = "VendorGoodsRefresh";
    #endregion
}