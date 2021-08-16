using UnityEngine;

[CreateAssetMenu(fileName = "medicine", menuName = "Zetan Studio/道具/药物")]
public class MedicineItem : ItemBase
{
    public MedicineItem()
    {
        itemType = ItemType.Medicine;
    }
}
