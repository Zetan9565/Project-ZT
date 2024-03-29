using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.DialogueSystem;
using ZetanStudio.QuestSystem;

namespace ZetanStudio.CharacterSystem
{
    [CreateAssetMenu(fileName = "talker info", menuName = "Zetan Studio/角色/谈话人信息")]
    public class TalkerInformation : NPCInformation, IKeyword
    {
        [SerializeField]
        private Dialogue defaultDialogue;
        public Dialogue DefaultDialogue => defaultDialogue;


        [SerializeField, NonReorderable]
        private List<ConditionDialogue> conditionDialogues = new List<ConditionDialogue>();
        public List<ConditionDialogue> ConditionDialogues => conditionDialogues;

        [SerializeField]
        private bool isWarehouseAgent;
        public bool IsWarehouseAgent => isWarehouseAgent;
        [SerializeField]
        private int warehouseCapcity = 30;
        public int WarehouseCapcity => warehouseCapcity;

        [SerializeField]
        private bool isVendor;
        public bool IsVendor => isVendor;
        [SerializeField]
        private ShopInformation shop;
        public ShopInformation Shop => shop;

        [SerializeField]
        private List<Quest> questsStored = new List<Quest>();
        public List<Quest> QuestsStored => questsStored;

        [SerializeField]
        private bool canDEV_RLAT;
        /// <summary>
        /// 可培养感情
        /// </summary>
        public bool CanDEV_RLAT => canDEV_RLAT;

        [SerializeField]
        private Dialogue normalItemDialogue;
        public Dialogue NormalItemDialogue => normalItemDialogue;

        [SerializeField]
        private int normalIntimacyValue = 5;
        public int NormalIntimacyValue => normalIntimacyValue;

        [SerializeField, NonReorderable]
        private List<AffectiveDialogue> giftDialogues = new List<AffectiveDialogue>();
        public List<AffectiveDialogue> GiftDialogues => giftDialogues;


        [SerializeField, NonReorderable]
        private List<AffectiveItemInfo> affectiveItems = new List<AffectiveItemInfo>();
        public List<AffectiveItemInfo> AffectiveItems => affectiveItems;

        [SerializeField]
        private bool canMarry;
        public bool CanMarry => canMarry;

        public override bool IsValid
        {
            get
            {
                return base.IsValid && (!enable || enable && !string.IsNullOrEmpty(scene) && prefab && defaultDialogue && (!isVendor || isVendor && shop) && questsStored.TrueForAll(x => x != null)
                    && conditionDialogues.TrueForAll(x => x.IsValid) && (!canDEV_RLAT || canDEV_RLAT && normalItemDialogue && giftDialogues.TrueForAll(x => x.Dialogue) && affectiveItems.TrueForAll(x => x.Item)));
            }
        }

        string IKeyword.IDPrefix => "NPC";

        Color IKeyword.Color => Color.cyan;

        string IKeyword.Group => "NPC";

        [GetKeywordsMethod, RuntimeGetKeywordsMethod]
        public static IEnumerable<TalkerInformation> GetTalkers()
        {
            return Resources.LoadAll<TalkerInformation>("Configuration");
        }
    }

    [System.Serializable]
    public class Relationship
    {
        [SerializeField]
        private int relationshipValue;
        public int RelationshipValue
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
}