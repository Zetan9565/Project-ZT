using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenu(fileName = "product info", menuName = "Zetan Studio/产出表")]
public class ProductInformation : ScriptableObject
{
    [SerializeField]
    private string remark;

    [SerializeField, NonReorderable]
    private List<DropItemInfo> products = new List<DropItemInfo>();
    public ReadOnlyCollection<DropItemInfo> Products => new ReadOnlyCollection<DropItemInfo>(products);

    public bool IsValid => products != null && products.Count > 0;

    public List<ItemWithAmount> DoDrop()
    {
        return DropItemInfo.Drop(products);
    }

    public string GetDropInfoString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < Products.Count; i++)
        {
            var di = Products[i];
            if(di.IsValid)
            {
                sb.Append("[");
                sb.Append(di.DropRate);
                sb.Append("%]概率掉落");
                sb.Append('[');
                sb.Append(di.ItemName);
                sb.Append("] ");
                sb.Append(di.MinAmount);
                if (di.MinAmount < di.MaxAmount)
                {
                    sb.Append("~");
                    sb.Append(di.MaxAmount);
                }
                sb.Append("个");
            }
            if (i != Products.Count - 1) sb.Append("\n");
        }
        return sb.ToString();
    }
}