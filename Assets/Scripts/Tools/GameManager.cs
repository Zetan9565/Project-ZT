using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{

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

    public static bool dontDestroyOnLoadOnce;

    private void Awake()
    {
        if (!dontDestroyOnLoadOnce)
        {
            DontDestroyOnLoad(this);
            dontDestroyOnLoadOnce = true;
            Init();
        }
        else
        {
            Destroy(gameObject);
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
                if (!allTalker.ContainsKey(talker.TalkerID))
                {
                    allTalker.Add(talker.TalkerID, talker);
                }
                else
                {
                    Debug.LogWarningFormat("[处理重复NPC] ID: {0}  Name: {1}", talker.TalkerID, talker.TalkerName);
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
            if (kvp.Value is QuestGiver) (kvp.Value as QuestGiver).Init();
    }

    public ItemBase GetItemByID(string id)
    {
        ItemBase[] items = Resources.LoadAll<ItemBase>("");
        if (items.Length < 1) return null;
        if (Array.Exists(items, x => x.ID == id))
            return Array.Find(items, x => x.ID == id);
        return null;
    }
}
