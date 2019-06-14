using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
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

    [SerializeField]
    private string backpackName = "行囊";
    public string BackpackName
    {
        get
        {
            return backpackName;
        }
    }

    [SerializeField]
    private string coinName = "文";
    public string CoinName
    {
        get
        {
            return coinName;
        }
    }

    [SerializeField]
    private List<Color> qualityColors = new List<Color>();
    public List<Color> QualityColors
    {
        get
        {
            return qualityColors;
        }
    }

    private static bool dontDestroyOnLoadOnce;

    private void Awake()
    {
        if (!dontDestroyOnLoadOnce)
        {
            DontDestroyOnLoad(this);
            dontDestroyOnLoadOnce = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Init();
    }

    public static Dictionary<string, List<Enemy>> Enermies { get; } = new Dictionary<string, List<Enemy>>();

    public static Dictionary<string, Talker> Talkers { get; } = new Dictionary<string, Talker>();

    public static Dictionary<string, QuestPoint> QuestPoints { get; } = new Dictionary<string, QuestPoint>();

    public static void Init()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        QuestManager.Instance.SetUI(FindObjectOfType<QuestUI>());
        BackpackManager.Instance.SetUI(FindObjectOfType<BackpackUI>());
        WarehouseManager.Instance.SetUI(FindObjectOfType<WarehouseUI>());
        DialogueManager.Instance.SetUI(FindObjectOfType<DialogueUI>());
        BuildingManager.Instance.SetUI(FindObjectOfType<BuildingUI>());
        ShopManager.Instance.SetUI(FindObjectOfType<ShopUI>());
        EscapeMenuManager.Instance.SetUI(FindObjectOfType<EscapeUI>());
        foreach (KeyValuePair<string, Talker> kvp in Talkers)
            if (kvp.Value is QuestGiver) (kvp.Value as QuestGiver).Init();
        PlayerManager.Instance.Init();
        WindowsManager.Instance.Clear();
    }

    public static ItemBase GetItemByID(string id)
    {
        ItemBase[] items = Resources.LoadAll<ItemBase>("");
        if (items.Length < 1) return null;
        return Array.Find(items, x => x.ID == id);
    }
}
