using System;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Module;

public class ItemCoolDown : CoolDown
{
    private ItemData item;

    public void Init(ItemData item)
    {
        this.item = item;
        ZetanUtility.SetActive(this, item);
        if (!item) Disable();
        else Enable();
    }

    protected override void OnAwake()
    {
        NotifyCenter.AddListener(BackpackManager.BackpackUseItem, OnUseItem, this);
    }

    private void OnUseItem(object[] msg)
    {
        if (msg[0] is ItemData item && item == this.item)
            Enable();
    }

    private void OnDestroy()
    {
        NotifyCenter.RemoveListener(this);
    }

    private CoolDownData CD => item.GetModuleData<CoolDownData>();

    protected override bool Active => item && CD && !CD.Available;


    public override Func<float> GetTime
    {
        get => () =>
        {
            return (CD is CoolDownData cool) ? cool.Time : 1;
        };
    }

    public override Func<float> GetTotal
    {
        get => () =>
        {
            return (CD is CoolDownData cool) ? cool.Module.Time : 1;
        };
    }
}
