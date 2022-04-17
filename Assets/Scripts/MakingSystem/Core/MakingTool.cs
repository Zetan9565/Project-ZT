using UnityEngine;

[DisallowMultipleComponent]
public class MakingTool : Building2D
{
    public MakingToolInformation ToolInfo { get; private set; }

    public override bool IsInteractive
    {
        get
        {
            return base.IsInteractive && ToolInfo && !NewWindowsManager.IsWindowOpen<MakingWindow>();
        }
    }

    protected override void OnNotInteractable()
    {
        if (NewWindowsManager.IsWindowOpen<MakingWindow>(out var making) && making.CurrentTool == this)
            making.Interrupt();
        base.OnNotInteractable();
    }

    public override bool DoManage()
    {
        return NewWindowsManager.OpenWindowBy<MakingWindow>(this, BackpackManager.Instance);
    }

    protected override void OnInit()
    {
        if (Info.Addendas.Count > 0)
        {
            if (Info.Addendas[0] is MakingToolInformation info)
                ToolInfo = info;
        }
    }
}