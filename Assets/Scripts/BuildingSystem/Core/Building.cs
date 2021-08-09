using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Building : InteractiveObject
{
    public BuildingData Data { get; private set; }
    public string EntityID => Data ? Data.entityID : string.Empty;

    public override string name
    {
        get
        {
            return Info ? Info.name : "未设置";
        }
    }

    public bool IsBuilt => Data && Data.IsBuilt;

    public override bool IsInteractive
    {
        get
        {
            return Info && base.IsInteractive && IsBuilt && !BuildingManager.Instance.IsManaging;
        }

        protected set
        {
            base.IsInteractive = value;
        }
    }

    public BuildingInformation Info => Data ? Data.Info : null;

    [SerializeField]
    protected UnityEvent onDestroy = new UnityEvent();

#if UNITY_EDITOR
    [ReadOnly]
#endif
    public BuildingAgent buildingAgent;

    public void Build(BuildingData data)
    {
        Data = data;
        Data.entity = this;
        gameObject.name = Data.name;
        transform.position = Data.position;
        hidePanelOnInteract = true;
        OnBuilt();
    }

    protected virtual void OnBuilt()
    {
        if (buildingAgent) buildingAgent.UpdateUI();
    }

    public virtual void Destroy()
    {
        onDestroy?.Invoke();
        Destroy(gameObject);
    }

    public virtual void OnCancelManage()
    {
        FinishInteraction();
    }

    public virtual void OnDoneManage()
    {
        BuildingManager.Instance.PauseDisplayInfo(false);
    }

    public virtual void OnManage()
    {
        if (Info.Manageable)
            BuildingManager.Instance.PauseDisplayInfo(true);
    }

    public override bool DoInteract()
    {
        if (BuildingManager.Instance.Manage(this))
            return base.DoInteract();
        return false;
    }

    protected override void OnExit(Collider2D collision)
    {
        if (collision.CompareTag("Player") && BuildingManager.Instance.CurrentBuilding == this)
        {
            BuildingManager.Instance.CancelManage();
        }
    }
}