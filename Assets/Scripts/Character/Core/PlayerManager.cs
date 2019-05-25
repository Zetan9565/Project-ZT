using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;
    public static PlayerManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<PlayerManager>();
            return instance;
        }
    }

    [SerializeField]
    private PlayerInfomation playerInfo;
    public PlayerInfomation PlayerInfo
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

    public void SetPlayerInfo(PlayerInfomation playerInfo)
    {
        this.playerInfo = playerInfo;
        Init();
    }

    public void Equip(ItemInfo toEquip)
    {
        if (toEquip == null || !toEquip.Item) return;
        ItemInfo equiped = null;
        switch (toEquip.Item.ItemType)
        {
            case ItemType.Weapon:
                BackpackManager.Instance.MBackpack.backpackSize--;//为将要替换出来的武器留出空间
                if (PlayerInfo.HasPrimaryWeapon && (toEquip.Item as WeaponItem).IsPrimary)
                {
                    equiped = PlayerInfo.UnequipWeapon(true);
                }
                else if (PlayerInfo.HasSecondaryWeapon && !(toEquip.Item as WeaponItem).IsPrimary)
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
                BackpackManager.Instance.MBackpack.weightLoad += toEquip.Item.Weight;
                BackpackManager.Instance.UpdateUI();
                MessageManager.Instance.NewMessage(string.Format("装备了 [{0}]", toEquip.ItemName));
                break;
            case ItemType.Armor:
                break;
            default: return;
        }
        if (BackpackManager.Instance.GetItem(equiped, 1))
        {
            BackpackManager.Instance.MBackpack.weightLoad -= equiped.Item.Weight;
            BackpackManager.Instance.UpdateUI();
        }
    }

    public void Unequip(ItemInfo toUnequip)
    {
        if (toUnequip == null) return;
        ItemInfo equiped = toUnequip;
        switch (toUnequip.Item.ItemType)
        {
            case ItemType.Weapon:
                BackpackManager.Instance.MBackpack.weightLoad -= equiped.Item.Weight;
                BackpackManager.Instance.MBackpack.backpackSize--;
                if (PlayerInfo.HasPrimaryWeapon && (equiped.Item as WeaponItem).IsPrimary)
                {
                    equiped = PlayerInfo.UnequipWeapon(true);
                }
                else if (PlayerInfo.HasSecondaryWeapon && !(equiped.Item as WeaponItem).IsPrimary)
                {
                    equiped = PlayerInfo.UnequipWeapon(false);
                }
                break;
            default: break;
        }
        if (!BackpackManager.Instance.TryGetItem_Boolean(equiped))
        {
            PlayerInfo.EquipWeapon(equiped);
            BackpackManager.Instance.MBackpack.weightLoad += equiped.Item.Weight;
            BackpackManager.Instance.MBackpack.backpackSize++;
            return;
        }
        BackpackManager.Instance.GetItem(equiped, 1);
    }
}