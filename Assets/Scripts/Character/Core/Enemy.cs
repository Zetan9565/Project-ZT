using UnityEngine;
using System.Collections.Generic;

public delegate void EnermyDeathListener();

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    public string EnemyID
    {
        get { return Info.ID; }
    }

    public string EnemyName
    {
        get { return info.Name; }
    }

    [SerializeField]
    private EnemyInformation info;
    public EnemyInformation Info
    {
        get
        {
            return info;
        }
    }

    public event EnermyDeathListener OnDeathEvent;

    private void Awake()
    {
        GameManager.Enemies.TryGetValue(EnemyID, out var enemies);
        if (enemies == null || enemies.Count < 1) GameManager.Enemies.Add(EnemyID, new List<Enemy>() { this });
        else if (!enemies.Contains(this)) GameManager.Enemies[EnemyID].Add(this);
    }

    public void Death()
    {
        //Debug.Log("One [" + info.Name + "] was killed");
        OnDeathEvent?.Invoke();
        QuestManager.Instance.UpdateUI();
        if (info.DropItems.Count > 0)
        {
            List<ItemInfo> lootItems = new List<ItemInfo>();
            foreach (DropItemInfo di in info.DropItems)
                if (ZetanUtil.Probability(di.DropRate))
                    if (!di.OnlyDropForQuest || (di.OnlyDropForQuest && QuestManager.Instance.HasOngoingQuestWithID(di.BindedQuest.ID)))
                        lootItems.Add(new ItemInfo(di.Item, Random.Range(1, di.Amount + 1)));
            if (lootItems.Count > 0)
            {
                LootAgent la = ObjectPool.Instance.Get(info.LootPrefab).GetComponent<LootAgent>();
                la.Init(lootItems, transform.position);
            }
        }
    }
}