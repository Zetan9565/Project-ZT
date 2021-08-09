using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gathering : InteractiveObject
{
    [SerializeField]
    protected GatheringInformation gatheringInfo;
    public GatheringInformation GatheringInfo => gatheringInfo;

    public override string name { get => gatheringInfo.name; protected set => _name = value; }

    [SerializeField]
    protected bool hideOnGathered;

    public override bool IsInteractive
    {
        get
        {
            return gatheringInfo && gatheringInfo.ProductItems.Count > 0 && GatherManager.Instance.Gathering != this;
        }
    }

    public virtual float LeftRefreshTime { get; protected set; }

    protected Coroutine refreshCoroutine;

    [HideInInspector]
    public UnityEngine.Events.UnityEvent onGatherFinish = new UnityEngine.Events.UnityEvent();

    public virtual void GatherSuccess()
    {
        onGatherFinish?.Invoke();
        if (hideOnGathered) GetComponent<Renderer>().enabled = false;
        if (GatheringInfo.ProductItems.Count > 0)
        {
            List<ItemInfoBase> lootItems = DropItemInfo.Drop(GatheringInfo.ProductItems);
            if (lootItems.Count > 0)
            {
                LootAgent la = ObjectPool.Get(GatheringInfo.LootPrefab).GetComponent<LootAgent>();
                la.Init(lootItems, transform.position);
            }
        }
        if (refreshCoroutine != null) StopCoroutine(refreshCoroutine);
        refreshCoroutine = StartCoroutine(UpdateTime());
    }

    protected virtual IEnumerator UpdateTime()
    {
        if (GatheringInfo.RefreshTime < 0) yield break;

        LeftRefreshTime = GatheringInfo.RefreshTime;
        while (!IsInteractive && GatheringInfo)
        {
            LeftRefreshTime -= Time.deltaTime;
            if (LeftRefreshTime <= 0)
            {
                LeftRefreshTime = GatheringInfo.RefreshTime;
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
