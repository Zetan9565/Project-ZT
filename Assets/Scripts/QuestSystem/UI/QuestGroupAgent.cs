using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class QuestGroupAgent : MonoBehaviour
{
    [HideInInspector]
    public QuestGroup questGroup;

    public Text nameText;

    public Transform questListParent;

    bool isExpanded;

    public List<QuestAgent> questAgents = new List<QuestAgent>();

    public void OnClick()
    {
        Expand(!isExpanded);
    }

    public void Expand(bool state)
    {
        if (!state)
        {
            MyTools.SetActive(questListParent.gameObject, false);
            isExpanded = false;
        }
        else
        {
            MyTools.SetActive(questListParent.gameObject, true);
            isExpanded = true;
        }
        UpdateStatus();
    }

    public void UpdateStatus()
    {
        if (questGroup)
            nameText.text = questGroup.Name + (isExpanded ? "<" : ">");
    }

    public void Recycle()
    {
        questAgents.Clear();
        questGroup = null;
        nameText.text = string.Empty;
        ObjectPool.Instance.Put(gameObject);
    }
}
