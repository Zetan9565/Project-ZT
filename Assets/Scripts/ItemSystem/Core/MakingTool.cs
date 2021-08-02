using UnityEngine;

[DisallowMultipleComponent]
public class MakingTool : Building
{
    public MakingToolInformation ToolInfo { get; private set; }

    public override bool IsInteractive
    {
        get
        {
            return base.IsInteractive && ToolInfo && !MakingManager.Instance.IsMaking;
        }
    }

    public override void OnCancelManage()
    {
        base.OnCancelManage();
        if (MakingManager.Instance.CurrentTool == this)
            MakingManager.Instance.CancelMake();
    }

    public override void OnManage()
    {
        base.OnManage();
        MakingManager.Instance.Make(this);
    }

    protected override void OnBuilt()
    {
        if (Info.Addendas.Count > 0)
        {
            if (Info.Addendas[0] is MakingToolInformation info)
                ToolInfo = info;
        }
    }
}