using System.Collections.Generic;
using UnityEngine;
using ZetanExtends;

[DisallowMultipleComponent]
public class Talker : Character<TalkerData>
{
    public string TalkerID => GetGenericData() ? GetGenericData().Info.ID : string.Empty;

    public string TalkerName => GetGenericData() ? GetGenericData().Info.Name : string.Empty;

    public Vector3 questFlagOffset;
    private QuestFlag flagAgent;

    public List<QuestData> QuestInstances => GetGenericData() ? GetGenericData().questInstances : null;

    [SerializeField]
    private MapIconHolder iconHolder;
    private Interactive interactive;

    public bool IsInteractive
    {
        get
        {
            return GetGenericData().Info && data && !DialogueManager.Instance.IsTalking;
        }
    }

    public Dialogue DefaultDialogue
    {
        get
        {
            foreach (var cd in GetGenericData().Info.ConditionDialogues)
            {
                if (MiscFuntion.CheckCondition(cd.Condition))
                    return cd.Dialogue;
            }
            return GetGenericData().Info.DefaultDialogue;
        }
    }

    public override void Init(TalkerData data)
    {
        base.Init(data);
        transform.position = GetGenericData().GetInfo<NPCInformation>().Position;
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
        GetGenericData()?.OnTalkBegin();
    }

    public void OnTalkFinished()
    {
        GetGenericData()?.OnTalkFinished();
    }

    public Dialogue OnGetGift(ItemBase gift)
    {
        return GetGenericData()?.OnGetGift(gift);
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
        if (GetGenericData().Info.IsVendor && GetGenericData().Info.Shop || GetGenericData().Info.IsWarehouseAgent && GetGenericData().Info.WarehouseCapcity > 0)
        {
            name.Append("<");
            if (GetGenericData().Info.IsVendor) name.Append(GetGenericData().Info.Shop.ShopName);
            if (GetGenericData().Info.IsVendor && GetGenericData().Info.IsWarehouseAgent) name.Append(",");
            if (GetGenericData().Info.IsWarehouseAgent) name.Append("仓库");
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
        Gizmos.DrawWireCube(base.transform.position + questFlagOffset, new Vector3(1, 1, 0));
    }
#endif
    #endregion
}
