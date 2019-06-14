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

    private bool isExpanded;
    public bool IsExpanded
    {
        get
        {
            return isExpanded;
        }
        set
        {
            if (!value)
            {
                MyUtilities.SetActive(questListParent.gameObject, false);
                isExpanded = false;
            }
            else
            {
                MyUtilities.SetActive(questListParent.gameObject, true);
                isExpanded = true;
            }
            UpdateStatus();
        }
    }

    public List<QuestAgent> questAgents = new List<QuestAgent>();

    public void OnClick()
    {
        IsExpanded = !IsExpanded;
    }

    public void UpdateStatus()
    {
        if (questGroup)
            nameText.text = questGroup.Name + (IsExpanded ? "<" : ">");
    }

    public void Recycle()
    {
        questAgents.Clear();
        questGroup = null;
        nameText.text = string.Empty;
        ObjectPool.Instance.Put(gameObject);
    }
}
