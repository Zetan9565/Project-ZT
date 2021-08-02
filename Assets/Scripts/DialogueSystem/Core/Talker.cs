using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(MapIconHolder), typeof(Interactive))]
public class Talker : Character
{
    public new TalkerInformation Info => (TalkerInformation)info;

    public string TalkerID => info ? info.ID : string.Empty;

    public string TalkerName => info ? info.name : string.Empty;

    public Vector3 questFlagOffset;
    private QuestFlag flagAgent;

    public new TalkerData Data
    {
        get => (TalkerData)data;
        set
        {
            data = value;
            base.Data = data;
        }
    }

    public List<QuestData> QuestInstances => Data ? Data.questInstances : null;

    [SerializeField]
    private MapIconHolder iconHolder;
    public Interactive Interactive { get; private set; }

    public new Transform transform { get; private set; }

    public bool IsInteractive
    {
        get
        {
            return info && data && !DialogueManager.Instance.IsTalking;
        }
    }

    public override bool Init()
    {
        if (!GameManager.Talkers.ContainsKey(TalkerID)) GameManager.Talkers.Add(TalkerID, this);
        else if (!GameManager.Talkers[TalkerID] || !GameManager.Talkers[TalkerID].gameObject)
        {
            GameManager.Talkers.Remove(TalkerID);
            GameManager.Talkers.Add(TalkerID, this);
        }
        else Destroy(gameObject);
        if (!GameManager.TalkerDatas.TryGetValue(TalkerID, out TalkerData dataFound))
        {
            Data = new TalkerData(Info);
            if (Info.IsVendor)
            {
                Data.shop = Instantiate(Info.Shop);
                Data.shop.Init();
            }
            else if (Info.IsWarehouseAgent) Data.warehouse = new WarehouseData(Info.WarehouseCapcity);
            Data.Info = Info;
            Data.InitQuest(Info.QuestsStored);
            GameManager.TalkerDatas.Add(TalkerID, Data);
        }
        else Data = dataFound;
        Data.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Data.currentPosition = transform.position;
        if (Info.IsVendor && !ShopManager.Vendors.Contains(Data)) ShopManager.Vendors.Add(Data);
        flagAgent = ObjectPool.Get(QuestManager.Instance.QuestFlagsPrefab.gameObject, UIManager.Instance.QuestFlagParent).GetComponent<QuestFlag>();
        flagAgent.Init(this);
        return true;
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

    public bool DoInteract()
    {
        return DialogueManager.Instance.Talk(this);
    }

    public void OnExit(Collider2D collision)
    {
        if (collision.CompareTag("Player") && DialogueManager.Instance.CurrentTalker == this)
            DialogueManager.Instance.CancelTalk();
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
        if (Info.IsVendor && Info.Shop || Info.IsWarehouseAgent && Info.WarehouseCapcity > 0)
        {
            name.Append("<");
            if (Info.IsVendor) name.Append(Info.Shop.ShopName);
            if (Info.IsVendor && Info.IsWarehouseAgent) name.Append(",");
            if (Info.IsWarehouseAgent) name.Append("仓库");
            name.Append(">");
        }
        return name.ToString();
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        Interactive = GetComponent<Interactive>();
        iconHolder = GetComponent<MapIconHolder>();
        if (iconHolder)
        {
            iconHolder.textToDisplay = GetMapIconName();
            iconHolder.iconEvents.RemoveAllListner();
            iconHolder.iconEvents.onFingerClick.AddListener(ShowNameAtMousePosition);
            iconHolder.iconEvents.onMouseEnter.AddListener(ShowNameAtMousePosition);
            iconHolder.iconEvents.onMouseExit.AddListener(HideNameImmediately);
        }
        transform = base.transform;
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
        if (flagAgent) flagAgent.Recycle();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(base.transform.position + questFlagOffset, Vector3.one);
    }
    #endregion
}
