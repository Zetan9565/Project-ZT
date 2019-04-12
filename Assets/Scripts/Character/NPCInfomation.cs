using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "npc info", menuName = "ZetanStudio/角色/NPC信息")]
public class NPCInfomation : CharacterInfomation
{
    [SerializeField]
    private List<ItemBase> favoriteItems;
    public List<ItemBase> FavoriteItems
    {
        get
        {
            return favoriteItems;
        }
    }

    [SerializeField]
    private List<ItemBase> hateItems;
    public List<ItemBase> HateItems
    {
        get
        {
            return hateItems;
        }
    }
}
