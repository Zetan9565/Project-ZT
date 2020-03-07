using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(MapIconHolder))]
public class Talker : MonoBehaviour
{
    [SerializeField]
    private TalkerInformation info;
    public TalkerInformation Info => info;

    public string TalkerID => info ? info.ID : string.Empty;

    public string TalkerName => info ? info.name : string.Empty;

    public Vector3 questFlagsOffset;
    private QuestFlag flagsAgent;

    public TalkerData Data { get; private set; }

    public List<Quest> QuestInstances => Data.questInstances;

    [SerializeField]
    private MapIconHolder iconHolder;

    public void Init()
    {
        if (!GameManager.Talkers.ContainsKey(TalkerID)) GameManager.Talkers.Add(TalkerID, this);
        else if (!GameManager.Talkers[TalkerID] || !GameManager.Talkers[TalkerID].gameObject)
        {
            GameManager.Talkers.Remove(TalkerID);
            GameManager.Talkers.Add(TalkerID, this);
        }
        else Destroy(gameObject);
        GameManager.TalkerDatas.TryGetValue(TalkerID, out TalkerData dataFound);
        if (!dataFound)
        {
            Data = new TalkerData();
            if (Info.IsVendor)
            {
                Data.shop = Instantiate(Info.Shop);
                Data.shop.Init();
            }
            else if (Info.IsWarehouseAgent) Data.warehouse = new Warehouse(Info.Warehouse.size.Max);
            Data.info = Info;
            Data.InitQuest(Info.QuestsStored);
            GameManager.TalkerDatas.Add(TalkerID, Data);
        }
        else Data = dataFound;
        Data.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Data.currentPosition = transform.position;
        if (Info.IsVendor && !ShopManager.Vendors.Contains(Data)) ShopManager.Vendors.Add(Data);
        flagsAgent = ObjectPool.Get(QuestManager.Instance.QuestFlagsPrefab.gameObject, UIManager.Instance.QuestFlagParent).GetComponent<QuestFlag>();
        flagsAgent.Init(this);
    }

    public void OnTalkBegin()
    {
        Data.OnTalkBegin();
    }

    public void OnTalkFinished()
    {
        Data.OnTalkFinished();
    }

    public void OnGetGift(ItemBase gift)
    {
        Data.OnGetGift(gift);
    }

    #region UI相关
    private void ShowNameAtMousePosition()
    {
        int time = -1;
#if UNITY_ANDROID
        time = 2;
#endif
        TipsManager.Instance.ShowText(Input.mousePosition, GetMapIconName(), time);
    }
    private void HideNameImmediately()
    {
        TipsManager.Instance.Hide();
    }
    private string GetMapIconName()
    {
        System.Text.StringBuilder name = new System.Text.StringBuilder(TalkerName);
        if (info.IsVendor && info.Shop || info.IsWarehouseAgent && info.Warehouse)
        {
            name.Append("<");
            if (info.IsVendor) name.Append(info.Shop.ShopName);
            if (info.IsVendor && info.IsWarehouseAgent) name.Append(",");
            if (info.IsWarehouseAgent) name.Append("仓库");
            name.Append(">");
        }
        return name.ToString();
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        iconHolder = GetComponent<MapIconHolder>();
        if (iconHolder)
        {
            iconHolder.textToDisplay = GetMapIconName();
            iconHolder.iconEvents.RemoveAllListner();
            iconHolder.iconEvents.onFingerClick.AddListener(ShowNameAtMousePosition);
            iconHolder.iconEvents.onMouseEnter.AddListener(ShowNameAtMousePosition);
            iconHolder.iconEvents.onMouseExit.AddListener(HideNameImmediately);
        }
    }

    private void Update()
    {
        if (Data) Data.currentPosition = transform.position;
    }

    private void OnValidate()
    {
        iconHolder = GetComponent<MapIconHolder>();
        if (iconHolder)
        {
            iconHolder.textToDisplay = GetMapIconName();
            iconHolder.iconEvents.onFingerClick.AddListener(ShowNameAtMousePosition);
            iconHolder.iconEvents.onMouseEnter.AddListener(ShowNameAtMousePosition);
            iconHolder.iconEvents.onMouseExit.AddListener(HideNameImmediately);
        }
    }

    private void OnDestroy()
    {
        if (flagsAgent) flagsAgent.Recycle();
    }

    #region 触发器相关
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !DialogueManager.Instance.IsTalking)
            DialogueManager.Instance.CanTalk(this);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !DialogueManager.Instance.IsTalking)
            DialogueManager.Instance.CanTalk(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && DialogueManager.Instance.CurrentTalker != null && DialogueManager.Instance.CurrentTalker == this)
            DialogueManager.Instance.CannotTalk();
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !DialogueManager.Instance.IsTalking)
            DialogueManager.Instance.CanTalk(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !DialogueManager.Instance.IsTalking)
            DialogueManager.Instance.CanTalk(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && DialogueManager.Instance.CurrentTalker != null && DialogueManager.Instance.CurrentTalker == this)
            DialogueManager.Instance.CannotTalk();
    }*/
    #endregion
    #endregion
}

[System.Serializable]
public class TalkerData
{
    public TalkerInformation info;
    public string TalkerID
    {
        get
        {
            if (info) return info.ID;
            return string.Empty;
        }
    }

    public string TalkerName
    {
        get
        {
            if (info) return info.name;
            return string.Empty;
        }
    }

    public string currentScene;
    public Vector3 currentPosition;

    public Relationship relationshipInstance;

    public Warehouse warehouse;

    public ShopInformation shop;

    public List<TalkObjective> objectivesTalkToThis = new List<TalkObjective>();
    public List<SubmitObjective> objectivesSubmitToThis = new List<SubmitObjective>();

    public delegate void DialogueListener();
    public event DialogueListener OnTalkBeginEvent;
    public event DialogueListener OnTalkFinishedEvent;

    public List<Quest> questInstances = new List<Quest>();

    public virtual void OnTalkBegin()
    {
        OnTalkBeginEvent?.Invoke();
    }

    public virtual void OnTalkFinished()
    {
        OnTalkFinishedEvent?.Invoke();
    }

    public void OnGetGift(ItemBase gift)
    {
        if (info.FavoriteItems.Exists(x => x.Item.ID == gift.ID))
        {
            FavoriteItemInfo find = info.FavoriteItems.Find(x => x.Item.ID == gift.ID);
            relationshipInstance.RelationshipValue.Current += (int)find.FavoriteLevel;
        }
        else if (info.HateItems.Exists(x => x.Item.ID == gift.ID))
        {
            HateItemInfo find = info.HateItems.Find(x => x.Item.ID == gift.ID);
            relationshipInstance.RelationshipValue.Current -= (int)find.HateLevel;
        }
        else
        {
            relationshipInstance.RelationshipValue.Current += 5;
        }
    }

    public void InitQuest(List<Quest> questsStored)
    {
        if (questsStored == null) return;
        if (questInstances.Count > 0) questInstances.Clear();
        foreach (Quest quest in questsStored)
        {
            if (quest)
            {
                Quest questInstance = Object.Instantiate(quest);
                foreach (CollectObjective co in questInstance.CollectObjectives)
                    if (co.IsValid) questInstance.ObjectiveInstances.Add(co);
                foreach (KillObjective ko in questInstance.KillObjectives)
                    if (ko.IsValid) questInstance.ObjectiveInstances.Add(ko);
                foreach (TalkObjective to in questInstance.TalkObjectives)
                    if (to.IsValid) questInstance.ObjectiveInstances.Add(to);
                foreach (MoveObjective mo in questInstance.MoveObjectives)
                    if (mo.IsValid) questInstance.ObjectiveInstances.Add(mo);
                foreach (SubmitObjective so in questInstance.SubmitObjectives)
                    if (so.IsValid) questInstance.ObjectiveInstances.Add(so);
                foreach (CustomObjective cuo in questInstance.CustomObjectives)
                    if (cuo.IsValid) questInstance.ObjectiveInstances.Add(cuo);
                questInstance.ObjectiveInstances.Sort((x, y) =>
                {
                    if (x.OrderIndex > y.OrderIndex) return 1;
                    else if (x.OrderIndex < y.OrderIndex) return -1;
                    else return 0;
                });
                if (quest.CmpltObjctvInOrder)
                    for (int i = 1; i < questInstance.ObjectiveInstances.Count; i++)
                    {
                        if (questInstance.ObjectiveInstances[i].OrderIndex >= questInstance.ObjectiveInstances[i - 1].OrderIndex)
                        {
                            questInstance.ObjectiveInstances[i].PrevObjective = questInstance.ObjectiveInstances[i - 1];
                            questInstance.ObjectiveInstances[i - 1].NextObjective = questInstance.ObjectiveInstances[i];
                        }
                    }
                int i1, i2, i3, i4, i5, i6;
                i1 = i2 = i3 = i4 = i5 = i6 = 0;
                foreach (Objective o in questInstance.ObjectiveInstances)
                {
                    if (o is CollectObjective)
                    {
                        o.runtimeID = questInstance.ID + "_CO" + i1;
                        i1++;
                    }
                    if (o is KillObjective)
                    {
                        o.runtimeID = questInstance.ID + "_KO" + i2;
                        i2++;
                    }
                    if (o is TalkObjective)
                    {
                        o.runtimeID = questInstance.ID + "_TO" + i3;
                        i3++;
                    }
                    if (o is MoveObjective)
                    {
                        o.runtimeID = questInstance.ID + "_MO" + i4;
                        i4++;
                    }
                    if (o is SubmitObjective)
                    {
                        o.runtimeID = questInstance.ID + "_SO" + i5;
                        i5++;
                    }
                    if (o is CustomObjective)
                    {
                        o.runtimeID = questInstance.ID + "_CUO" + i6;
                        i6++;
                    }
                    o.runtimeParent = questInstance;
                }
                questInstance.originalQuestHolder = this;
                questInstance.currentQuestHolder = this;
                questInstances.Add(questInstance);
            }
        }
    }

    public void TransferQuestToThis(Quest quest)
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