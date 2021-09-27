using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : InteractiveObject
{
    [SerializeField]
    protected ResourceInformation resourceInfo;
    public ResourceInformation ResourceInfo => resourceInfo;

    protected override string CustomName { get => resourceInfo.Name; }

    [SerializeField]
    protected bool hideOnGathered;

    public override bool IsInteractive
    {
        get
        {
            return resourceInfo && resourceInfo.ProductItems.Count > 0 && GatherManager.Instance.Gathering != this;
        }
    }

    public virtual float LeftRefreshTime { get; protected set; }

    protected Coroutine refreshCoroutine;

    [HideInInspector]
    public UnityEngine.Events.UnityEvent onGatherFinish = new UnityEngine.Events.UnityEvent();

    private void Awake()
    {
        customName = true;
    }

    public virtual void GatherSuccess()
    {
        onGatherFinish?.Invoke();
        if (hideOnGathered) GetComponent<Renderer>().enabled = false;
        if (ResourceInfo.ProductItems.Count > 0)
        {
            List<ItemInfoBase> lootItems = DropItemInfo.Drop(ResourceInfo.ProductItems);
            if (lootItems.Count > 0)
            {
                LootAgent la = ObjectPool.Get(ResourceInfo.LootPrefab).GetComponent<LootAgent>();
                la.Init(lootItems, transform.position);
            }
        }
        if (refreshCoroutine != null) StopCoroutine(refreshCoroutine);
        refreshCoroutine = StartCoroutine(UpdateTime());
    }

    protected virtual IEnumerator UpdateTime()
    {
        if (ResourceInfo.RefreshTime < 0) yield break;

        LeftRefreshTime = ResourceInfo.RefreshTime;
        while (!IsInteractive && ResourceInfo)
        {
            LeftRefreshTime -= Time.deltaTime;
            if (LeftRefreshTime <= 0)
            {
                LeftRefreshTime = ResourceInfo.RefreshTime;
                Refresh();
                yield break;
            }
            yield return null;
        }
    }

    public virtual void Refresh()
    {
        IsInteractive = true;
        if (hideOnGathered) GetComponent<Renderer>().enabled = true;
    }

    public override bool DoInteract()
    {
        if (GatherManager.Instance.Gather(this))
        {
            return base.DoInteract();
        }
        return false;
    }

    protected override void OnExit(Collider2D collision)
    {
        if (collision.CompareTag("Player") && GatherManager.Instance.Gathering == this)
        {
            GatherManager.Instance.Cancel();
        }
    }
}
