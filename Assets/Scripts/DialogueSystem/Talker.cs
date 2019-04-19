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
    public Relationship Relationship { get; private set; }

    /// <summary>
    /// 存储对象身上的对话型目标
    /// </summary>
    [HideInInspector]
    public List<TalkObjective> talkToThisObjectives = new List<TalkObjective>();

    public event DialogueListener OnTalkBeginEvent;
    public event DialogueListener OnTalkFinishedEvent;

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
        if(info.FavoriteItems.Exists(x=>x.Item.ID == gift.ID))
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
}