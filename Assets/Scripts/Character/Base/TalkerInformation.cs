using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "npc info", menuName = "ZetanStudio/角色/NPC信息")]
public class TalkerInformation : CharacterInformation
{
    [SerializeField]
    private Dialogue defaultDialogue;
    public Dialogue DefaultDialogue
    {
        get
        {
            return defaultDialogue;
        }
    }

    [SerializeField]
    private bool canDEV_RLAT;
    public bool CanDEV_RLAT
    {
        get
        {
            return canDEV_RLAT;
        }
    }

    [SerializeField]
    private Dialogue normalItemDialogue;
    public Dialogue NormalItemDialogue
    {
        get
        {
            return normalItemDialogue;
        }
    }

    [SerializeField]
    private AffectiveDialogue favoriteItemDialogue;
    public AffectiveDialogue FavoriteItemDialogue
    {
        get
        {
            return favoriteItemDialogue;
        }
    }

    [SerializeField]
    private AffectiveDialogue hateItemDialogue;
    public AffectiveDialogue HateItemDialogue
    {
        get
        {
            return hateItemDialogue;
        }
    }

    [SerializeField]
    private List<FavoriteItemInfo> favoriteItems;
    public List<FavoriteItemInfo> FavoriteItems
    {
        get
        {
            return favoriteItems;
        }
    }

    [SerializeField]
    private List<HateItemInfo> hateItems;
    public List<HateItemInfo> HateItems
    {
        get
        {
            return hateItems;
        }
    }


    [SerializeField]
    private bool canMarry;
    public bool CanMarry
    {
        get
        {
            return canMarry;
        }
    }
}

[System.Serializable]
public class Relationship
{
    [SerializeField]
    private ScopeInt relationshipValue = new ScopeInt(-500, 20000);
    public ScopeInt RelationshipValue
    {
        get
        {
            return relationshipValue;
        }

        set
        {
            relationshipValue = value;
            if (relationshipValue <= -300) RelationshipLevel = RelationshipLevel.VeryBad;
            else if (relationshipValue <= -100 && relationshipValue > -300) RelationshipLevel = RelationshipLevel.Bad;
            else if (relationshipValue < 0 && relationshipValue > -100) RelationshipLevel = RelationshipLevel.NotGood;
            else if (relationshipValue == 0) RelationshipLevel = RelationshipLevel.FirstMeet;
            else if (relationshipValue > 0 && relationshipValue < 100) RelationshipLevel = RelationshipLevel.Normal;
            else if (relationshipValue >= 100 && relationshipValue < 500) RelationshipLevel = RelationshipLevel.Good;
            else if (relationshipValue >= 500 && relationshipValue < 1500) RelationshipLevel = RelationshipLevel.VeryGood;
            else if (relationshipValue >= 5000 && relationshipValue < 15000) RelationshipLevel = RelationshipLevel.Lover;
            else RelationshipLevel = RelationshipLevel.Spouse;
        }
    }

    [SerializeField]
    private RelationshipLevel relationshipLevel;
    public RelationshipLevel RelationshipLevel
    {
        get
        {
            return relationshipLevel;
        }
        private set
        {
            relationshipLevel = value;
        }
    }
}
public enum RelationshipLevel
{
    FirstMeet,//萍水相逢
    VeryBad,//深恶痛绝
    Bad,//厌恶
    NotGood,//冷漠
    Normal,//普通
    Good,//熟人
    VeryGood,//知己
    Lover,//恋人
    Spouse//结发
}
public enum SpouseRLATLevel
{
    落花难上枝,
    破镜重圆,
    鸾凤和鸣,
    举案齐眉,
    白头偕老,
}