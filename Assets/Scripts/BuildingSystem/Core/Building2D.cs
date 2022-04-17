using UnityEngine;
using UnityEngine.Events;
using ZetanExtends;

[DisallowMultipleComponent]
public class Building2D : Interactive2D
{
    public BuildingData Data { get; private set; }
    public string EntityID => Data ? Data.ID : string.Empty;

    public override string Name
    {
        get
        {
            return Info ? Info.Name : "未设置";
        }
    }

    public bool IsBuilt => Data && Data.IsBuilt;

    public override bool IsInteractive
    {
        get
        {
            return Info && IsBuilt && !(NewWindowsManager.IsWindowOpen<BuidingManageWindow>(out var building) && building.IsManaging);
        }
    }

    public BuildingInformation Info => Data ? Data.Info : null;

    [SerializeField]
    protected UnityEvent onDestroy = new UnityEvent();

    public void Init(BuildingData data)
    {
        Data = data;
        Data.entity = this;
        gameObject.name = Data.Name;
        transform.position = Data.position;
        OnInit();
    }

    protected virtual void OnInit()
    {
        if (Data.buildingAgent) Data.buildingAgent.UpdateUI();
    }

    public virtual void Destroy()
    {
        onDestroy?.Invoke();
        Destroy(gameObject);
    }

    /// <summary>
    /// 由建筑自身的各类功能窗口调用，如<see cref="WarehouseWindow.OnClose(object[])"/>中对<see cref="WarehouseWindow.warehouse"/>的操作
    /// </summary>
    public virtual void EndManagement()
    {
        if (Info.Manageable)
            NewWindowsManager.HideWindow<BuidingManageWindow>(false, this);
    }

    /// <summary>
    /// 由<see cref="BuidingManageWindow.OnClose(object[])"/>调用
    /// </summary>
    protected override void OnEndInteraction()
    {
        NewWindowsManager.HideWindow<BuidingManageWindow>(false, this);
    }

    /// <summary>
    /// 由<see cref="BuidingManageWindow.ManageCurrent"/>调用
    /// </summary>
    public virtual bool DoManage()
    {
        return true;
    }

    public override bool DoInteract()
    {
        return NewWindowsManager.OpenWindow<BuidingManageWindow>(this);
    }

    /// <summary>
    /// 重写时需要调用基类<see cref="OnNotInteractable"/>
    /// </summary>
    protected override void OnNotInteractable()
    {
        if (NewWindowsManager.IsWindowOpen<BuidingManageWindow>(out var manager) && manager.Target == this)
            manager.Interrupt();
    }
}