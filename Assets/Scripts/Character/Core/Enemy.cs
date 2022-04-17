using UnityEngine;
using System.Collections.Generic;


[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    public string EnemyID => info ? info.ID : string.Empty;

    public string EnemyName => info ? info.Name : string.Empty;

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
        if (info.DropItems && info.DropItems.IsValid)
        {
            var lootItems = info.DropItems.DoDrop();
            if (lootItems.Count > 0)
            {
                LootAgent la = ObjectPool.Get(info.LootPrefab).GetComponent<LootAgent>();
                la.Init(lootItems, transform.position);
            }
        }
    }
}