using UnityEngine;

[DisallowMultipleComponent]
public class MakingTool : Building
{
    public MakingToolInformation Info { get; private set; }

    public override bool Interactive
    {
        get
        {
            return base.Interactive && Info && !MakingManager.Instance.IsMaking;
        }
    }

    public override void OnCancelManage()
    {
        if (MakingManager.Instance.CurrentTool == this)
            MakingManager.Instance.CancelMake();
    }

    public override void OnManage()
    {
        MakingManager.Instance.Make(this);
    }

    protected override void OnBuilt()
    {
        if (MBuildingInfo.Addendas.Count > 0)
        {
            if (MBuildingInfo.Addendas[0] is MakingToolInformation info)
                Info = info;
        }
    }
}