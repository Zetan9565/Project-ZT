using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherAgent : MonoBehaviour
{
    [SerializeField]
    private GatheringInformation gatheringInfo;
    public GatheringInformation GatheringInfo
    {
        get
        {
            return gatheringInfo;
        }
    }

    private bool gatherAble = true;
    private bool GatherAble
    {
        get
        {
            return gatherAble && gatheringInfo;
        }

        set
        {
            gatherAble = value;
        }
    }

    private float leftRefreshTime;

    [HideInInspector]
    public UnityEngine.Events.UnityEvent onGatherFinish = new UnityEngine.Events.UnityEvent();

    public void GatherSuccess()
    {
        onGatherFinish?.Invoke();
        GatherAble = false;
        if (GatheringInfo.ProductItems.Count > 0)
        {
            List<ItemInfo> lootItems = new List<ItemInfo>();
            foreach (DropItemInfo di in GatheringInfo.ProductItems)
                if (MyUtilities.Probability(di.DropRate))
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

    private IEnumerator UpdateTime()
    {
        leftRefreshTime = GatheringInfo.RefreshTime;
        while (!GatherAble && GatheringInfo)
        {
            leftRefreshTime -= Time.deltaTime;
            if (leftRefreshTime <= 0)
            {
                GatherAble = true;
                leftRefreshTime = GatheringInfo.RefreshTime;
                yield break;
            }
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!GatherAble || !GatheringInfo) return;
        if (collision.CompareTag("Player") && !GatherManager.Instance.IsGathering)
        {
            GatherManager.Instance.CanGather(this);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!GatherAble || !GatheringInfo) return;
        if (collision.CompareTag("Player") && !GatherManager.Instance.IsGathering)
        {
            GatherManager.Instance.CanGather(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && GatherManager.Instance.GatherAgent == this)
        {
            GatherManager.Instance.CannotGather();
        }
    }
}
