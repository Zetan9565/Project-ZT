using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "formulation", menuName = "Zetan Studio/配方")]
public class Formulation : ScriptableObject
{
    /**
    * 设计思路：
    * 材料列表里面的材料分为【单种】和【同类】，【单种】限制一种道具，比如铁块，而为同类时，可以是铁块、铜块等“金属”
    * 当材料中某个同类材料时，就不应该再增加任何该类型下面的单种道具材料，很明显会冲突
    * 同理，当材料列表已经用了某种单种材料，则不应该再增加与材料类型一样的【同类】材料
    * 材料中，无论是【单种】和【同类】，都不应该再增加相同的配置，例如，已经使用了铁块X3，则不能再新增一个铁块X2的材料配置
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
        return CheckMaterialsDuplicate(left.materials, right.materials);
    }
    public static bool CheckMaterialsDuplicate(IEnumerable<MaterialInfo> itemMaterials, IEnumerable<MaterialInfo> otherMaterials)
    {
        if (itemMaterials == null || itemMaterials.Count() < 1 || otherMaterials == null || otherMaterials.Count() < 1 || itemMaterials.Count() != otherMaterials.Count()) return false;
        using (var materialEnum = itemMaterials.GetEnumerator())
            while (materialEnum.MoveNext())
            {
                var material = materialEnum.Current;
                if (material.MakingType == MakingType.SingleItem)
                {
                    var find = otherMaterials.FirstOrDefault(x => x.Item == material.Item);
                    if (!find || find.Amount != material.Amount) return false;
                }
            }
        foreach (MaterialType type in Enum.GetValues(typeof(MaterialType)))
        {
            int amout1 = itemMaterials.Where(x => x.MakingType == MakingType.SameType && x.MaterialType == type).Select(x => x.Amount).Sum();
            int amout2 = otherMaterials.Where(x => x.MakingType == MakingType.SameType && x.MaterialType == type).Select(x => x.Amount).Sum();
            if (amout1 != amout2) return false;
        }
        return true;
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var material in materials)
        {
            if (material && material.IsValid)
            {
                if (material.MakingType == MakingType.SingleItem)
                {
                    sb.Append("[");
                    sb.Append(material.ItemName);
                    sb.Append("] × ");
                    sb.Append(material.Amount);
                }
                else
                {
                    sb.Append("<");
                    sb.Append(ZetanUtility.GetEnumInspectorName(material.MaterialType));
                    sb.Append("> × ");
                    sb.Append(material.Amount);
                }
                if (materials.IndexOf(material) != materials.Count - 1) sb.Append("\n");
            }
        }
        return sb.ToString();
    }
}