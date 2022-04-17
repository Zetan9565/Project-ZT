using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class QuestAgent : ListItem<QuestAgent, QuestAgentData>
{
    [SerializeField] private GameObject groupContent;
    //public Image groupIcon;
    [SerializeField] private Text groupText;
    [SerializeField] private SubQuestList questList;
    public SubQuestList SubList => questList;

    [SerializeField] private GameObject questContent;
    //public Image questIcon;
    [SerializeField] private Text questText;

    [SerializeField] private GameObject selected;

    protected override void RefreshSelected()
    {
        ZetanUtility.SetActive(selected, IsSelected);
        ZetanUtility.SetActive(questList, Data.group && IsSelected);
        if (!IsSelected) questList.DeselectAll();
    }

    public override void Refresh()
    {
        if (!Data.group)
        {
            ZetanUtility.SetActive(groupContent, false);
            if (Data.quests.Count > 0)
            {
                ZetanUtility.SetActive(questContent, true);
                var quest = base.Data.quests[0];
                questText.text = quest.IsFinished ? quest.Model.Title : (quest.IsComplete ? $"{quest.Model.Title}(已完成)" :
                    (quest.InProgress ? $"{quest.Model.Title}(进行中)" : $"{quest.Model.Title}(未接取)"));
            }
            questList.Clear();
        }
        else
        {
            ZetanUtility.SetActive(groupContent, true);
            groupText.text = Data.group.Name;
            questList.Refresh(Convert(Data.quests));
            ZetanUtility.SetActive(questContent, false);
        }
    }

    private static List<QuestAgentData> Convert(IEnumerable<QuestData> quests)
    {
        List<QuestAgentData> results = new List<QuestAgentData>();
        foreach (var quest in quests)
        {
            results.Add(new QuestAgentData(quest));
        }
        return results;
    }

    public override void OnClear()
    {
        base.OnClear();
        questList.Clear();
    }
}
public class QuestAgentData
{
    public readonly QuestGroup group;
    public readonly List<QuestData> quests;

    public QuestAgentData(QuestData quest)
    {
        quests = new List<QuestData>() { quest };
    }
    public QuestAgentData(QuestGroup group, IEnumerable<QuestData> quests)
    {
        this.group = group;
        this.quests = new List<QuestData>(quests);
    }
}