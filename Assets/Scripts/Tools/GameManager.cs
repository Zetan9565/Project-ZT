using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/游戏管理器")]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField]
    private string backpackName = "背包";
    public static string BackpackName
    {
        get
        {
            if (Instance) return Instance.backpackName;
            else return "背包";
        }
    }

    [SerializeField]
    private string coinName = "铜币";
    public static string CoinName
    {
        get
        {
            if (Instance) return Instance.coinName;
            else return "铜币";
        }
    }

    [SerializeField]
    private List<Color> qualityColors = new List<Color>();
    public static List<Color> QualityColors
    {
        get
        {
            if (Instance)
                return Instance.qualityColors;
            else return null;
        }
    }

    private static bool dontDestroyOnLoadOnce;

    [SerializeField]
    private UIManager UIRootPrefab;
    private void Awake()
    {
        if (!dontDestroyOnLoadOnce)
        {
            DontDestroyOnLoad(this);
            dontDestroyOnLoadOnce = true;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private void Start()
    {
        InitGame();
    }

    public static Dictionary<CropInformation, List<Crop>> Crops { get; } = new Dictionary<CropInformation, List<Crop>>();

    public static Dictionary<string, List<Enemy>> Enemies { get; } = new Dictionary<string, List<Enemy>>();

    public static Dictionary<string, Talker> Talkers { get; } = new Dictionary<string, Talker>();
    public static Dictionary<string, TalkerData> TalkerDatas { get; } = new Dictionary<string, TalkerData>();

    public static Dictionary<string, QuestPoint> QuestPoints { get; } = new Dictionary<string, QuestPoint>();

    public static void InitGame(params Type[] exceptions)
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        ActionStack.Clear();
        Crops.Clear();
        foreach (var enemykvp in Enemies)
            enemykvp.Value.RemoveAll(x => !x || !x.gameObject);
        if (exceptions == null || !exceptions.Contains(typeof(Talker)))
        {
            Talkers.Clear();
            TalkerDatas.Clear();
            foreach (var talker in FindObjectsOfType<Talker>())
                talker.Init();
        }
        if (exceptions == null || !exceptions.Contains(typeof(TriggerHolder)))
            foreach (var tholder in FindObjectsOfType<TriggerHolder>())
                tholder.Init();
        PlayerManager.Instance.Init();
        if (!UIManager.Instance || !UIManager.Instance.gameObject) Instantiate(Instance.UIRootPrefab);
        UIManager.Instance.Init();
        QuestManager.Instance.Init();
        MessageManager.Instance.Init();
        MapManager.Instance.Init();
        MapManager.Instance.SetPlayer(PlayerManager.Instance.PlayerTransform);
        MapManager.Instance.RemakeCamera();
        GatherManager.Instance.Init();
        WindowsManager.Instance.Clear();
    }

    /// <summary>
    /// 尝试获取道具（非新的实例）
    /// </summary>
    /// <param name="id">道具ID</param>
    /// <returns>获得的道具</returns>
    public static ItemBase GetItemByID(string id)
    {
        ItemBase[] items = Resources.LoadAll<ItemBase>("");
        if (items.Length < 1) return null;
        return Array.Find(items, x => x.ID == id);
    }

    public static Color QualityToColor(ItemQuality quality)
    {
        if (quality > 0 && QualityColors != null && (int)quality < QualityColors.Count)
            return QualityColors[(int)quality];
        else return Color.black;
    }
}