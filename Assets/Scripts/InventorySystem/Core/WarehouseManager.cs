public class WarehouseManager : SingletonInventoryHandler<WarehouseManager>
{
    public IWarehouseKeeper Warehouse { get; private set; }
    public override Inventory Inventory { get => Warehouse != null ? Warehouse.Inventory : base.Inventory; protected set => base.Inventory = value; }

    public void SetManagedWarehouse(IWarehouseKeeper warehouse)
    {
        ListenInventoryChange(false);
        Warehouse = warehouse;
        ListenInventoryChange(true);
    }
    #region 消息相关
    public override string InventoryMoneyChangedMsgKey => WarehouseMoneyChanged;

    public override string InventorySpaceChangedMsgKey => WarehouseSpaceChanged;

    public override string InventoryWeightChangedMsgKey => WarehouseWeightChanged;

    public override string ItemAmountChangedMsgKey => WarehouseItemAmountChanged;
    public override string SlotStateChangedMsgKey => WarehouseSlotStateChanged;


    public const string WarehouseMoneyChanged = "WarehouseMoneyChanged";
    public const string WarehouseSpaceChanged = "WarehouseSpaceChanged";
    public const string WarehouseWeightChanged = "WarehouseWeightChanged";
    public const string WarehouseItemAmountChanged = "WarehouseItemAmountChanged";
    public const string WarehouseSlotStateChanged = "WarehouseSlotStateChanged";
    #endregion
}