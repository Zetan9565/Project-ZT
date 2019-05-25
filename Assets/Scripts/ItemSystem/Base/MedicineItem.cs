using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "medicine", menuName = "ZetanStudio/道具/药物")]
public class MedicineItem : ItemBase
{
    public MedicineItem()
    {
        itemType = ItemType.Medicine;
    }
}
