using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(MapIconHolder), typeof(Interactive))]
public class Talker : Character
{
    public string TalkerID => Data ? Data.Info.ID : string.Empty;

    public string TalkerName => Data ? Data.Info.name : string.Empty;

    public Vector3 questFlagOffset;
    private QuestFlag flagAgent;

    public new TalkerData Data
    {
        get => data as TalkerData;
        set
        {
            data = value;
        }
    }

    public List<QuestData> QuestInstances => Data ? Data.questInstances : null;

    [SerializeField]
    private MapIconHolder iconHolder;
    private Interactive interactive;

    public bool IsInteractive
    {
        get
        {
            return Data.Info && data && !DialogueManager.Instance.IsTalking;
        }
    }

    public Dialogue DefaultDialogue
    {
        get
        {
            foreach (var cd in Data.Info.ConditionDialogues)
            {
                if (MiscFuntion.CheckCondition(cd.Condition))
                    return cd.Dialogue;
            }
            return Data.Info.DefaultDialogue;
        }
    }

    public void Init(TalkerData data)
    {
        Data = data;
        Data.entity = this;
        transform.position = Data.Info.Position;
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
        Data?.OnTalkBegin();
    }

    public void OnTalkFinished()
    {
        Data?.OnTalkFinished();
    }

    public Dialogue OnGetGift(ItemBase gift)
    {
        return Data?.OnGetGift(gift);
    }

    public bool DoInteract()
    {
        if (DialogueManager.Instance.Talk(this))
        {
            SetState(CharacterState.Busy, CharacterBusyState.Talking);
            return true;
        }
        return false;
    }
    public void FinishInteraction()
    {
        SetState(CharacterState.Normal, CharacterNormalState.Idle);
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
        if (Data.Info.IsVendor && Data.Info.Shop || Data.Info.IsWarehouseAgent && Data.Info.WarehouseCapcity > 0)
        {
            name.Append("<");
            if (Data.Info.IsVendor) name.Append(Data.Info.Shop.ShopName);
            if (Data.Info.IsVendor && Data.Info.IsWarehouseAgent) name.Append(",");
            if (Data.Info.IsWarehouseAgent) name.Append("仓库");
            name.Append(">");
        }
        return name.ToString();
    }
    #endregion

    #region MonoBehaviour
    protected override void OnAwake()
    {
        interactive = GetComponent<Interactive>();
        iconHolder = GetComponent<MapIconHolder>();
    }

    protected override void OnDestroy_()
    {
        if (flagAgent) flagAgent.Recycle();
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(base.transform.position + questFlagOffset, Vector3.one);
    }
    #endif
    #endregion
}
