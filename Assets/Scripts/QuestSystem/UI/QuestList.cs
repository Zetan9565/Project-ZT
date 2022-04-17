using System.Collections;
using UnityEngine.UI;

public class QuestList : ScrollListView<QuestAgent, QuestAgentData>
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