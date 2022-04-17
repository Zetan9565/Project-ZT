using UnityEngine;

[DisallowMultipleComponent]
public class Warehouse : Building2D
{
    [SerializeField]
#if UNITY_EDITOR
    [ReadOnly(true)]
#endif
    private int defaultSize = 50;
    public WarehouseData WData { get; private set; }

    public override bool IsInteractive
    {
        get
        {
            return Data && base.IsInteractive && !NewWindowsManager.IsWindowOpen<WarehouseWindow>();
        }
    }

    public override bool DoManage()
    {
        return NewWindowsManager.OpenWindow<WarehouseWindow>(WarehouseWindow.OpenType.Store, this, NewWindowsManager.FindWindow<BackpackWindow>());
    }

    protected override void OnNotInteractable()
    {
        if (IsBuilt && NewWindowsManager.IsWindowOpen<WarehouseWindow>(out var warehouse) && warehouse.Handler.Inventory == WData.Inventory)
            warehouse.Close();
        base.OnNotInteractable();
    }

    protected override void OnInit()
    {
        base.OnInit();
        WData = new WarehouseData(Info, transform.position, defaultSize)
        {
            entity = this,
            ID = EntityID,
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
        };
    }
}
public interface IWarehouseKeeper
{
    string WarehouseName { get; }
    Inventory Inventory { get; }
}