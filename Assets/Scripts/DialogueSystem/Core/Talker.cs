using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.Extension;
using ZetanStudio.ItemSystem;

[DisallowMultipleComponent]
public class Talker : Character, IInteractive
{
    //[SerializeReference, ReadOnly]
    protected TalkerData data;

    public string TalkerID => GetData<TalkerData>() ? GetData<TalkerData>().Info.ID : string.Empty;

    public string TalkerName => GetData<TalkerData>() ? GetData<TalkerData>().Info.Name : string.Empty;

    public Vector3 questFlagOffset;
    private QuestFlag flagAgent;

    public List<QuestData> QuestInstances => GetData<TalkerData>() ? GetData<TalkerData>().questInstances : null;

    [SerializeField]
    private MapIconHolder iconHolder;
    public Transform interactive;

    public bool IsInteractive
    {
        get
        {
            return GetData<TalkerData>().Info && data && !WindowsManager.IsWindowOpen<DialogueWindow>();
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

    public Sprite Icon => null;

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
        flagAgent = ObjectPool.Get(MiscSettings.Instance.QuestFlagsPrefab, UIManager.Instance.QuestFlagParent);
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

    public Dialogue OnGetGift(Item gift)
    {
        return GetData<TalkerData>()?.OnGetGift(gift);
    }

    public bool DoInteract()
    {
        if (DialogueWindow.TalkWith(this))
        {
            SetMachineState<CharacterTalkingState>();
            return true;
        }
        return false;
    }
    public void EndInteraction()
    {
        interactable = false;
        SetMachineState<CharacterIdleState>();
    }

    private void OnNotInteractable()
    {
        if (WindowsManager.IsWindowOpen<DialogueWindow>(out var dialogue) && dialogue.Target == this)
            dialogue.CancelTalk();
    }

    #region UI相关
    private void ShowNameAtMousePosition()
    {
        int time = -1;
#if UNITY_ANDROID
        time = 2;
#endif
        FloatTipsPanel.ShowText(Input.mousePosition, GetMapIconName(), time);
    }
    private void HideNameImmediately()
    {
        WindowsManager.CloseWindow<FloatTipsPanel>();
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
        if (!interactive) interactive = transform.FindOrCreate("Interactive");
        var collider = interactive.GetOrAddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 1.0f;
        iconHolder = GetComponentInChildren<MapIconHolder>();
    }

    protected override void OnDestroy_()
    {
        if (flagAgent) flagAgent.Recycle();
    }

    private bool interactable;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!interactable && IsInteractive && collision.CompareTag("Player"))
        {
            InteractionPanel.Instance.Insert(this);
            interactable = true;
        }
    }
    protected override void OnTriggerStay2D(Collider2D collision)
    {
        if (!interactable && IsInteractive && collision.CompareTag("Player"))
        {
            InteractionPanel.Instance.Insert(this);
            interactable = true;
        }
    }
    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (interactable && IsInteractive && collision.CompareTag("Player"))
        {
            InteractionPanel.Instance.Remove(this);
            interactable = false;
            OnNotInteractable();
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position + questFlagOffset, new Vector3(1, 1, 0));
    }
#endif
    #endregion
}
