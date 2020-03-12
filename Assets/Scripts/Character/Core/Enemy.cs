using UnityEngine;
using System.Collections.Generic;


[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    public string EnemyID => info ? info.ID : string.Empty;

    public string EnemyName => info ? info.name : string.Empty;

    [SerializeField]
    private EnemyInformation info;
    public EnemyInformation Info => info;

    public delegate void EnermyDeathListener();
    public event EnermyDeathListener OnDeathEvent;

    private void Awake()
    {
        GameManager.Enemies.TryGetValue(EnemyID, out var enemies);
        if (enemies == null) GameManager.Enemies.Add(EnemyID, new List<Enemy>() { this });
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
                if (ZetanUtility.Probability(di.DropRate))
                    if (!di.OnlyDropForQuest || (di.OnlyDropForQuest && QuestManager.Instance.HasOngoingQuestWithID(di.BindedQuest.ID)))
                        lootItems.Add(new ItemInfo(di.Item, Random.Range(1, di.Amount + 1)));
            if (lootItems.Count > 0)
            {
                LootAgent la = ObjectPool.Get(info.LootPrefab).GetComponent<LootAgent>();
                la.Init(lootItems, transform.position);
            }
        }
    }
}