using UnityEngine.UI;

public class SubQuestList : ListView<QuestAgent, QuestAgentData>
{
    protected override void RefreshOverrideCellSize()
    {
        var layoutGroup = this.layoutGroup as HorizontalOrVerticalLayoutGroup;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        applyCellSize = cellSize;
        ForEach(RefreshCellSize);
    }
}