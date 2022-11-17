using System.Collections.Generic;
using ZetanStudio.DialogueSystem;
using ZetanStudio.ItemSystem;
using ZetanStudio.ShopSystem;
using ZetanStudio.QuestSystem;
using ZetanStudio.InventorySystem;

namespace ZetanStudio.CharacterSystem
{
    public class TalkerData : CharacterData, IWarehouseKeeper
    {
        public TalkerInformation Info => GetInfo<TalkerInformation>();

        public string TalkerID
        {
            get
            {
                if (Info) return Info.ID;
                return string.Empty;
            }
        }

        public string TalkerName
        {
            get
            {
                if (Info) return Info.Name;
                return string.Empty;
            }
        }

        public Relationship relationshipInstance;

        public Inventory Inventory { get; private set; }

        public string WarehouseName => $"{TalkerName}的仓库";

        public ShopData shop;

        public List<TalkObjectiveData> objectivesTalkToThis = new List<TalkObjectiveData>();
        public List<SubmitObjectiveData> objectivesSubmitToThis = new List<SubmitObjectiveData>();

        public delegate void DialogueListener();
        public event DialogueListener OnTalkBeginEvent;
        public event DialogueListener OnTalkFinishedEvent;

        public List<QuestData> questInstances = new List<QuestData>();

        public TalkerData(TalkerInformation info) : base(info)
        {
            if (Info.IsVendor)
            {
                shop = new ShopData(this, info.Shop);
                ShopManager.Vendors.Add(this);
            }
            if (Info.IsWarehouseAgent)
            {
                Inventory = new Inventory(Info.WarehouseCapcity, ignoreLock: true);
            }
            relationshipInstance = new Relationship();
            InitQuest();
        }

        public virtual void OnTalkBegin()
        {
            OnTalkBeginEvent?.Invoke();
        }

        public virtual void OnTalkFinished()
        {
            OnTalkFinishedEvent?.Invoke();
        }

        public Dialogue OnGetGift(Item gift)
        {
            int add = 0;
            if (Info.AffectiveItems.Exists(x => x.Item.ID == gift.ID))
            {
                AffectiveItemInfo find = Info.AffectiveItems.Find(x => x.Item.ID == gift.ID);
                add = find.IntimacyValue;
            }
            else
            {
                add = Info.NormalIntimacyValue;
            }
            relationshipInstance.RelationshipValue += add;
            foreach (var ad in Info.GiftDialogues)
            {
                if (ad.LowerBound < add && add < ad.UpperBound)
                    return ad.Dialogue;
            }
            return Info.NormalItemDialogue;
        }

        private void InitQuest()
        {
            if (!Info || Info.QuestsStored == null) return;
            if (questInstances.Count > 0) questInstances.Clear();
            foreach (Quest quest in Info.QuestsStored)
            {
                if (quest)
                {
                    QuestData questInstance = new QuestData(quest)
                    {
                        originalQuestHolder = this,
                        currentQuestHolder = this
                    };
                    questInstances.Add(questInstance);
                }
            }
        }

        public void TryRemoveObjective(ObjectiveData objective, bool befCmplt)
        {
            if (!befCmplt && objective.IsComplete)
                if (objective is TalkObjectiveData || objective is SubmitObjectiveData)
                    if (objectivesTalkToThis.Contains(objective as TalkObjectiveData))
                        objectivesTalkToThis.RemoveAll(x => x == objective as TalkObjectiveData);
                    else if (objectivesSubmitToThis.Contains(objective as SubmitObjectiveData))
                        objectivesSubmitToThis.RemoveAll(x => x == objective as SubmitObjectiveData);
        }

        public void TransferQuest(QuestData quest)
        {
            if (!quest) return;
            questInstances.Add(quest);
            quest.currentQuestHolder.questInstances.Remove(quest);
            quest.currentQuestHolder = this;
        }

        public static implicit operator bool(TalkerData self)
        {
            return self != null;
        }
    }
}