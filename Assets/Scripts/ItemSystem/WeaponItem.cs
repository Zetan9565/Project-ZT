using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ZetanStudio/道具/武器")]
[System.Serializable]
public class WeaponItem : ItemBase, IUsable
{
    public WeaponItem()
    {
        itemType = ItemType.Weapon;
    }
    public void OnUse()
    {
        Debug.Log("UseWeapon");
    }
}