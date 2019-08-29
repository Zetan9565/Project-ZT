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
        if (!GameManager.Enermies.ContainsKey(EnemyID)) GameManager.Enermies.Add(EnemyID, new List<Enemy>() { this });
        else if (!GameManager.Enermies[EnemyID].Contains(this)) GameManager.Enermies[EnemyID].Add(this);
        else if (GameManager.Enermies[EnemyID].Exists(x => !x.gameObject)) GameManager.Enermies[EnemyID].RemoveAll(x => !x.gameObject);
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
                if (ZetanUtilities.Probability(di.DropRate))
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