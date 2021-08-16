using UnityEngine;

[DisallowMultipleComponent]
public class Warehouse : Building
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
            return Data && base.IsInteractive && !WarehouseManager.Instance.Managing;
        }

        protected set
        {
            base.IsInteractive = value;
        }
    }

    public override void OnManage()
    {
        base.OnManage();
        WarehouseManager.Instance.Manage(WData);
    }

    public override void OnCancelManage()
    {
        base.OnCancelManage();
        if (WarehouseManager.Instance.CurrentData == WData && isActiveAndEnabled && IsBuilt)
        {
            WarehouseManager.Instance.CancelManage();
        }
    }

    protected override void OnBuilt()
    {
        base.OnBuilt();
        WData = new WarehouseData(defaultSize)
        {
            entity = this,
            entityID = EntityID,
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            position = transform.position
        };
    }

    private void OnDestroy()
    {
        if (WarehouseManager.Instance)
            if (WarehouseManager.Instance.CurrentData == WData && isActiveAndEnabled && IsBuilt)
            {
                WarehouseManager.Instance.CancelManage();
            }
    }
}
