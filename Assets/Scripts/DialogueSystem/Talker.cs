using System.Collections.Generic;
using UnityEngine;

public delegate void DialogueListener();

[DisallowMultipleComponent]
public class Talker : MonoBehaviour
{
    [SerializeField]
    private NPCInfomation info;
    public NPCInfomation Info
    {
        get
        {
            return info;
        }
    }

    [SerializeField]
    private Dialogue defaultDialogue;
    public Dialogue DefaultDialogue
    {
        get
        {
            return defaultDialogue;
        }
    }

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
}