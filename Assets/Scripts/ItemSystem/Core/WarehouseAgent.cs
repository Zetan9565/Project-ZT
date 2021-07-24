using UnityEngine;

[DisallowMultipleComponent]
public class WarehouseAgent : Building
{
    [SerializeField]
    private Warehouse warehouse = new Warehouse(50);
    public Warehouse MWarehouse => warehouse;

    public override bool Interactive
    {
        get
        {
            return warehouse && base.Interactive && !WarehouseManager.Instance.Managing;
        }

        protected set
        {
            base.Interactive = value;
        }
    }

    public override void OnManage()
    {
        base.OnManage();
        WarehouseManager.Instance.Manage(this);
    }

    public override void OnCancelManage()
    {
        base.OnCancelManage();
        if (WarehouseManager.Instance.MWarehouse == MWarehouse && isActiveAndEnabled && IsBuilt)
        {
            WarehouseManager.Instance.CancelManage();
        }
    }

    public override void AskDestroy()
    {
        ConfirmManager.Instance.New(string.Format("{0}{1}\n内的东西不会保留，确定拆除吗？", name, ((Vector2)transform.position).ToString()),
            delegate { BuildingManager.Instance.DestroyBuilding(this); });
    }

    private void OnDestroy()
    {
        if (WarehouseManager.Instance)
            if (WarehouseManager.Instance.MWarehouse == MWarehouse && isActiveAndEnabled && IsBuilt)
            {
                WarehouseManager.Instance.CancelManage();
            }
    }
}
