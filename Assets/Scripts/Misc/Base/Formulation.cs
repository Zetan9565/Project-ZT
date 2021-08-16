using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "formulation", menuName = "Zetan Studio/�䷽")]
public class Formulation : ScriptableObject
{
    /**
    * ���˼·��
    * �����б�����Ĳ��Ϸ�Ϊ�����֡��͡�ͬ�ࡿ�������֡�����һ�ֵ��ߣ��������飬��Ϊͬ��ʱ�����������顢ͭ��ȡ�������
    * ��������ĳ��ͬ�����ʱ���Ͳ�Ӧ���������κθ���������ĵ��ֵ��߲��ϣ������Ի��ͻ
    * ͬ���������б��Ѿ�����ĳ�ֵ��ֲ��ϣ���Ӧ�����������������һ���ġ�ͬ�ࡿ����
    * �����У������ǡ����֡��͡�ͬ�ࡿ������Ӧ����������ͬ�����ã����磬�Ѿ�ʹ��������X3������������һ������X2�Ĳ�������
    * **/
    [SerializeField]
    private string remark;//�������ڱ�ע

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
                    sb.Append("] �� ");
                    sb.Append(material.Amount);
                }
                else
                {
                    sb.Append("<");
                    sb.Append(ZetanUtility.GetEnumInspectorName(material.MaterialType));
                    sb.Append("> �� ");
                    sb.Append(material.Amount);
                }
                if (materials.IndexOf(material) != materials.Count - 1) sb.Append("\n");
            }
        }
        return sb.ToString();
    }
}