using System.Collections.Generic;
using UnityEngine;

public delegate void DialogueListener();

[DisallowMultipleComponent]
public class Talker : MonoBehaviour
{
    [SerializeField]
    private TalkerInfomation info;
    public TalkerInfomation Info
    {
        get
        {
            return info;
        }
    }

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
            if (info) return info.Name;
            return string.Empty;
        }
    }

    public Relationship Relationship { get; private set; }

    [SerializeField]
    private bool isWarehouseAgent;
    public bool IsWarehouseAgent
    {
        get
        {
            return isWarehouseAgent && !isVendor;
        }
    }

    public Warehouse warehouse = new Warehouse();

    [SerializeField]
    private bool isVendor;
    public bool IsVendor
    {
        get
        {
            return isVendor && !isWarehouseAgent;
        }
    }

    public Shop shop = new Shop();

    /// <summary>
    /// 存储对象身上的对话型目标
    /// </summary>
    [HideInInspector]
    public List<TalkObjective> objectivesTalkToThis = new List<TalkObjective>();

    public event DialogueListener OnTalkBeginEvent;
    public event DialogueListener OnTalkFinishedEvent;

    private void Awake()
    {
        if (IsVendor && !ShopManager.Vendors.Contains(this)) ShopManager.Vendors.Add(this);
        if (!GameManager.Talkers.ContainsKey(TalkerID)) GameManager.Talkers.Add(TalkerID, this);
        else if (!GameManager.Talkers[TalkerID] || !GameManager.Talkers[TalkerID].gameObject)
        {
            GameManager.Talkers.Remove(TalkerID);
            GameManager.Talkers.Add(TalkerID, this);
        }
        if (IsVendor) shop.Init();
    }

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
            Relationship.RelationshipValue += (int)find.FavoriteLevel;
        }
        else if (info.HateItems.Exists(x => x.Item.ID == gift.ID))
        {
            HateItemInfo find = info.HateItems.Find(x => x.Item.ID == gift.ID);
            Relationship.RelationshipValue -= (int)find.HateLevel;
        }
        else
        {
            Relationship.RelationshipValue += 5;
        }
    }

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
        if (collision.CompareTag("Player") && DialogueManager.Instance.CurrentTalker == this)
            DialogueManager.Instance.CannotTalk();
    }

    private void OnTriggerEnter(Collider other)
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
        if (other.CompareTag("Player") && DialogueManager.Instance.CurrentTalker == this)
            DialogueManager.Instance.CannotTalk();
    }
}