using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.QuestSystem.UI;
using ZetanStudio.UI;

namespace ZetanStudio.QuestSystem.UI
{
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
            Utility.SetActive(selected, IsSelected);
            Utility.SetActive(questList, Data != null && Data.group && IsSelected);
            if (!IsSelected) questList.ClearSelection();
        }
        public override void Refresh()
        {
            if (!Data.group)
            {
                Utility.SetActive(groupContent, false);
                if (Data.quests.Count > 0)
                {
                    Utility.SetActive(questContent, true);
                    var quest = Data.quests[0];
                    questText.text = quest.IsSubmitted ? quest.Title : (quest.IsComplete ? $"{LM.Tr(GetType().Name, "[已完成]")}{quest.Title}" :
                        (quest.InProgress ? quest.Title : $"{LM.Tr(GetType().Name, "[未接取]")}{quest.Title}"));
                }
                questList.Clear();
            }
            else
            {
                Utility.SetActive(groupContent, true);
                groupText.text = Data.group.Name;
                questList.Refresh(Convert(Data.quests));
                Utility.SetActive(questContent, false);
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

        public override void Clear()
        {
            base.Clear();
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
}