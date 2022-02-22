using System.Collections.Generic;
using UnityEngine;
using ZetanExtends;

[DisallowMultipleComponent]
public class Talker : Character
{
    [SerializeReference, ReadOnly]
    protected TalkerData data;

    public string TalkerID => GetData<TalkerData>() ? GetData<TalkerData>().Info.ID : string.Empty;

    public string TalkerName => GetData<TalkerData>() ? GetData<TalkerData>().Info.Name : string.Empty;

    public Vector3 questFlagOffset;
    private QuestFlag flagAgent;

    public List<QuestData> QuestInstances => GetData<TalkerData>() ? GetData<TalkerData>().questInstances : null;

    [SerializeField]
    private MapIconHolder iconHolder;
    private Interactive interactive;

    public bool IsInteractive
    {
        get
        {
            return GetData<TalkerData>().Info && data && !DialogueManager.Instance.IsTalking;
        }
    }

    public Dialogue DefaultDialogue
    {
        get
        {
            foreach (var cd in GetData<TalkerData>().Info.ConditionDialogues)
            {
                if (MiscFuntion.CheckCondition(cd.Condition))
                    return cd.Dialogue;
            }
            return GetData<TalkerData>().Info.DefaultDialogue;
        }
    }


    public override CharacterData GetData()
    {
        return data;
    }

    public override void SetData(CharacterData value)
    {
        data = (TalkerData)value;
    }

    public void Init(TalkerData data)
    {
        base.Init(data);
        transform.position = GetData<TalkerData>().GetInfo<NPCInformation>().Position;
        flagAgent = ObjectPool.Get(QuestManager.Instance.QuestFlagsPrefab.gameObject, UIManager.Instance.QuestFlagParent).GetComponent<QuestFlag>();
        flagAgent.Init(this);
        if (iconHolder)
        {
            iconHolder.textToDisplay = GetMapIconName();
            iconHolder.iconEvents.RemoveAllListner();
            iconHolder.iconEvents.onFingerClick.AddListener(ShowNameAtMousePosition);
            iconHolder.iconEvents.onMouseEnter.AddListener(ShowNameAtMousePosition);
            iconHolder.iconEvents.onMouseExit.AddListener(HideNameImmediately);
        }
    }

    public void OnTalkBegin()
    {
        GetData<TalkerData>()?.OnTalkBegin();
    }

    public void OnTalkFinished()
    {
        GetData<TalkerData>()?.OnTalkFinished();
    }

    public Dialogue OnGetGift(ItemBase gift)
    {
        return GetData<TalkerData>()?.OnGetGift(gift);
    }

    public bool DoInteract()
    {
        if (DialogueManager.Instance.Talk(this))
        {
            SetState(CharacterStates.Busy, CharacterBusyStates.Talking);
            return true;
        }
        return false;
    }
    public void FinishInteraction()
    {
        SetState(CharacterStates.Normal, CharacterNormalStates.Idle);
        interactive.FinishInteraction();
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
        TipsManager.Instance.ShowText(InputManager.mousePosition, GetMapIconName(), time);
    }
    private void HideNameImmediately()
    {
        TipsManager.Instance.Hide();
    }
    private string GetMapIconName()
    {
        System.Text.StringBuilder name = new System.Text.StringBuilder(TalkerName);
        if (GetData<TalkerData>().Info.IsVendor && GetData<TalkerData>().Info.Shop || GetData<TalkerData>().Info.IsWarehouseAgent && GetData<TalkerData>().Info.WarehouseCapcity > 0)
        {
            name.Append("<");
            if (GetData<TalkerData>().Info.IsVendor) name.Append(GetData<TalkerData>().Info.Shop.ShopName);
            if (GetData<TalkerData>().Info.IsVendor && GetData<TalkerData>().Info.IsWarehouseAgent) name.Append(",");
            if (GetData<TalkerData>().Info.IsWarehouseAgent) name.Append("仓库");
            name.Append(">");
        }
        return name.ToString();
    }
    #endregion

    #region MonoBehaviour
    protected override void OnAwake()
    {
        interactive = GetComponentInChildren<Interactive>();
        if (!interactive)
        {
            interactive = transform.FindOrCreate("Interactive").GetOrAddComponent<Interactive>();
            interactive.interactFunc = () => DoInteract();
            interactive.interactiveFunc = () => IsInteractive;
            interactive.getNameFunc = () => TalkerName;
            interactive.OnExit2D.AddListener(OnExit);
            var collider = interactive.GetOrAddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 1.0f;
        }
        iconHolder = GetComponentInChildren<MapIconHolder>();
    }

    protected override void OnDestroy_()
    {
        if (flagAgent) flagAgent.Recycle();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position + questFlagOffset, new Vector3(1, 1, 0));
    }
#endif
    #endregion
}
