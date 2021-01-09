using UnityEngine;
using UnityEditor;

[System.Serializable]
public class EquipmentItem : ItemBase
{
    [SerializeField]
    private RoleAttributeGroup attribute;
    public RoleAttributeGroup Attribute => attribute;

    [SerializeField]
    [Range(0, 2)]
    private int gemSlotAmount = 0;
    public int GemSlotAmout
    {
        get
        {
            return gemSlotAmount;
        }
    }
}