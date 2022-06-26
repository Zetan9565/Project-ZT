using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenu(fileName = "product info", menuName = "Zetan Studio/产出表")]
public class ProductInformation : ScriptableObject
{
    [SerializeField]
    private string remark;

    [SerializeField]
    private List<DropItemInfo> products = new List<DropItemInfo>();
    public ReadOnlyCollection<DropItemInfo> Products => new ReadOnlyCollection<DropItemInfo>(products);

    public bool IsValid => products != null && products.Count > 0;

    public List<CountedItem> DoDrop()
    {
        return DropItemInfo.Drop(products);
    }

    public string GetDropInfoString() => DropItemInfo.GetDropInfoString(products);
}