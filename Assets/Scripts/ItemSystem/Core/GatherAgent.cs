using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherAgent : MonoBehaviour
{
    [SerializeField]
    protected GatheringInformation gatheringInfo;
    public GatheringInformation GatheringInfo => gatheringInfo;

    private bool gatherAble = true;
    public bool GatherAble
    {
        get
        {
            return gatherAble && gatheringInfo;
        }

        protected set
        {
            gatherAble = value;
        }
    }

    public float LeftRefreshTime { get; protected set; }

    [HideInInspector]
    public UnityEngine.Events.UnityEvent onGatherFinish = new UnityEngine.Events.UnityEvent();

    public virtual void GatherSuccess()
    {
        onGatherFinish?.Invoke();
        GetComponent<Renderer>().enabled = false;
        GatherAble = false;
        if (GatheringInfo.ProductItems.Count > 0)
        {
            List<ItemInfo> lootItems = new List<ItemInfo>();
            foreach (DropItemInfo di in GatheringInfo.ProductItems)
                if (ZetanUtil.Probability(di.DropRate))
                    if (!di.OnlyDropForQuest || (di.OnlyDropForQuest && QuestManager.Instance.HasOngoingQuestWithID(di.BindedQuest.ID)))
                        lootItems.Add(new ItemInfo(di.Item, Random.Range(1, di.Amount + 1)));
            if (lootItems.Count > 0)
            {
                LootAgent la = ObjectPool.Instance.Get(GatheringInfo.LootPrefab).GetComponent<LootAgent>();
                la.Init(lootItems, transform.position);
            }
        }
        StartCoroutine(UpdateTime());
    }

    protected IEnumerator UpdateTime()
    {
        LeftRefreshTime = GatheringInfo.RefreshTime;
        while (!GatherAble && GatheringInfo)
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
        GatherAble = true;
        GetComponent<Renderer>().enabled = true;
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (!GatherAble || !GatheringInfo) return;
        if (other.CompareTag("Player") && !GatherManager.Instance.IsGathering)
        {
            GatherManager.Instance.CanGather(this);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!GatherAble || !GatheringInfo) return;
        if (other.CompareTag("Player") && !GatherManager.Instance.IsGathering)
        {
            GatherManager.Instance.CanGather(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && GatherManager.Instance.GatherAgent == this)
        {
            GatherManager.Instance.CannotGather();
        }
    }*/

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!GatherAble || !GatheringInfo) return;
        if (collision.CompareTag("Player") && !GatherManager.Instance.IsGathering)
        {
            GatherManager.Instance.CanGather(this);
        }
    }
    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (!GatherAble || !GatheringInfo) return;
        if (collision.CompareTag("Player") && !GatherManager.Instance.IsGathering)
        {
            GatherManager.Instance.CanGather(this);
        }
    }
    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && GatherManager.Instance.GatherAgent == this)
        {
            GatherManager.Instance.CannotGather();
        }
    }
}
