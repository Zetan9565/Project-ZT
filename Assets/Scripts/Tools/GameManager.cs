using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour {

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    private Dictionary<string, List<Enemy>> allEnermy = new Dictionary<string, List<Enemy>>();
    public Dictionary<string, List<Enemy>> AllEnermy
    {
        get
        {
            allEnermy.Clear();
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in enemies)
            {
                if (!allEnermy.ContainsKey(enemy.Info.ID))
                {
                    allEnermy.Add(enemy.Info.ID, new List<Enemy>());
                }
                allEnermy[enemy.Info.ID].Add(enemy);
            }
            return allEnermy;
        }
    }

    private Dictionary<string, Talker> allTalker = new Dictionary<string, Talker>();
    public Dictionary<string, Talker> AllTalker
    {
        get
        {
            allTalker.Clear();
            Talker[] normalTalkers = FindObjectsOfType<Talker>();
            foreach (Talker talker in normalTalkers)
            {
                try
                {
                    allTalker.Add(talker.Info.ID, talker);
                }
                catch
                {
                    Debug.LogWarningFormat("[处理NPC出错] ID: {0}  Name: {1}", talker.Info.ID, talker.Info.Name);
                    continue;
                }
            }
            return allTalker;
        }
    }

    private Dictionary<string, QuestPoint> allQuestPoint = new Dictionary<string, QuestPoint>();
    public Dictionary<string, QuestPoint> AllQuestPoint
    {
        get
        {
            allQuestPoint.Clear();
            QuestPoint[] questPoints = FindObjectsOfType<QuestPoint>();
            foreach (QuestPoint point in questPoints)
            {
                try
                {
                    allQuestPoint.Add(point.ID, point);
                }
                catch
                {
                    Debug.LogWarningFormat("[处理任务点出错] ID: {0}", point.ID);
                    continue;
                }
            }
            return allQuestPoint;
        }
    }

    public void Init()
    {
        foreach (KeyValuePair<string, Talker> kvp in AllTalker)
            if(kvp.Value is QuestGiver) (kvp.Value as QuestGiver).Init();
    }

    private void Awake()
    {
        Init();
    }

    public ItemBase GetItemInstanceByID(string id)
    {
        ItemBase itemInstance = Instantiate(Array.Find(Resources.LoadAll<ItemBase>(""), x => x.ID == id));
        if (itemInstance != null)
            switch (itemInstance.ItemType)
            {
                case ItemType.Weapon: return itemInstance as WeaponItem;
                case ItemType.Box: return itemInstance as ItemBox;
                default: return itemInstance;
            }
        return itemInstance;
    }
}
