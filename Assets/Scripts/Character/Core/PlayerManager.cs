using UnityEngine;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    [SerializeField]
    private PlayerInformation playerInfo;
    public PlayerInformation PlayerInfo
    {
        get
        {
            return playerInfo;
        }

        private set
        {
            playerInfo = value;
        }
    }


    [SerializeField]
    private PlayerController2D playerController;
    public PlayerController2D PlayerController
    {
        get
        {
            return playerController;
        }
        private set { playerController = value; }
    }

    public Backpack Backpack { get { return PlayerInfo.backpack; } }


    public void Init()
    {
        if (playerInfo)
        {
            playerInfo = Instantiate(playerInfo);
            BackpackManager.Instance.Init();
        }
    }

    public void SetPlayerInfo(PlayerInformation playerInfo)
    {
        this.playerInfo = playerInfo;
        Init();
    }

    public void Equip(ItemInfo toEquip)
    {
        if (toEquip == null || !toEquip.item) return;
        ItemInfo equiped = null;
        switch (toEquip.item.ItemType)
        {
            case ItemType.Weapon:
                BackpackManager.Instance.MBackpack.backpackSize--;//为将要替换出来的武器留出空间
                if (PlayerInfo.HasPrimaryWeapon && (toEquip.item as WeaponItem).IsPrimary)
                {
                    equiped = PlayerInfo.UnequipWeapon(true);
                }
                else if (PlayerInfo.HasSecondaryWeapon && !(toEquip.item as WeaponItem).IsPrimary)
                {
                    equiped = PlayerInfo.UnequipWeapon(false);
                }
                if ((equiped != null && !BackpackManager.Instance.TryGetItem_Boolean(equiped, 1)) || !PlayerInfo.EquipWeapon(toEquip))
                {
                    PlayerInfo.EquipWeapon(equiped);
                    BackpackManager.Instance.MBackpack.backpackSize++;
                    return;
                }
                BackpackManager.Instance.LoseItem(toEquip);
                BackpackManager.Instance.MBackpack.weightLoad += toEquip.item.Weight;
                BackpackManager.Instance.UpdateUI();
                MessageManager.Instance.NewMessage(string.Format("装备了 [{0}]", toEquip.ItemName));
                break;
            case ItemType.Armor:
                break;
            default: return;
        }
        if (BackpackManager.Instance.GetItem(equiped, 1))
        {
            BackpackManager.Instance.MBackpack.weightLoad -= equiped.item.Weight;
            BackpackManager.Instance.UpdateUI();
        }
    }

    public void Unequip(ItemInfo toUnequip)
    {
        if (toUnequip == null) return;
        ItemInfo equiped = toUnequip;
        switch (toUnequip.item.ItemType)
        {
            case ItemType.Weapon:
                BackpackManager.Instance.MBackpack.weightLoad -= equiped.item.Weight;
                BackpackManager.Instance.MBackpack.backpackSize--;
                if (PlayerInfo.HasPrimaryWeapon && (equiped.item as WeaponItem).IsPrimary)
                {
                    equiped = PlayerInfo.UnequipWeapon(true);
                }
                else if (PlayerInfo.HasSecondaryWeapon && !(equiped.item as WeaponItem).IsPrimary)
                {
                    equiped = PlayerInfo.UnequipWeapon(false);
                }
                break;
            default: break;
        }
        if (!BackpackManager.Instance.TryGetItem_Boolean(equiped))
        {
            PlayerInfo.EquipWeapon(equiped);
            BackpackManager.Instance.MBackpack.weightLoad += equiped.item.Weight;
            BackpackManager.Instance.MBackpack.backpackSize++;
            return;
        }
        BackpackManager.Instance.GetItem(equiped, 1);
    }
}