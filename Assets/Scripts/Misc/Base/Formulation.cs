using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.ItemSystem;

[CreateAssetMenu(fileName = "formulation", menuName = "Zetan Studio/配方")]
public class Formulation : ScriptableObject
{
    /**
    * 设计思路：
    * 材料列表里面的材料分为【单种】和【同类】，【单种】限制一种道具，比如铁块，而为同类时，可以是铁块、铜块等“金属”
    * 当材料中某个同类材料时，就不应该再增加任何该类型下面的单种道具材料，很明显会冲突
    * 同理，当材料列表已经用了某种单种材料，则不应该再增加与材料类型一样的【同类】材料
    * 材料中，无论是【单种】和【同类】，都不应该再增加相同的配置，例如，已经使用了[铁块 × m]，则不能再新增一个[铁块 × n]的材料配置
    * **/
    [SerializeField]
    private string remark;//单纯用于备注

    [SerializeField, NonReorderable]
    private List<MaterialInfo> materials = new List<MaterialInfo>();
    public virtual List<MaterialInfo> Materials => materials;

    public bool IsValid
    {
        get
        {
            return materials.TrueForAll(x => x && x.IsValid);
        }
    }

    public static bool CheckMaterialsDuplicate(Formulation left, Formulation right)
    {
        if (!left || !right) return false;
        return MaterialInfo.CheckMaterialsDuplicate(left.materials, right.materials);
    }

    public override string ToString()
    {
        if (!IsValid) return name;
        return ToSplitString(", ");
    }
    public string ToSplitString(string seperator = "\n")
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var material in materials)
        {
            if (material && material.IsValid)
            {
                if (material.CostType == MaterialCostType.SingleItem)
                {
                    sb.Append("[");
                    sb.Append(material.ItemName);
                    sb.Append("] × ");
                    sb.Append(material.Amount);
                }
                else
                {
                    sb.Append("<");
                    sb.Append(material.MaterialType.Name);
                    sb.Append("> × ");
                    sb.Append(material.Amount);
                }
                if (materials.IndexOf(material) != materials.Count - 1) sb.Append(seperator);
            }
        }
        return sb.ToString();
    }
}