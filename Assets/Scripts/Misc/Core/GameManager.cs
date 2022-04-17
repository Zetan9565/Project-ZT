using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Zetan Studio/管理器/游戏管理器")]
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

    private static bool dontDestroyOnLoadOnce;

    public static bool IsExiting { get; private set; }

    [SerializeField]
    private UIManager UIPrefab;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (!dontDestroyOnLoadOnce)
        {
            DontDestroyOnLoad(this);
            dontDestroyOnLoadOnce = true;
            StartCoroutine(InitDelay());
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        IsExiting = true;
    }

    private IEnumerator InitDelay()
    {
        yield return new WaitForEndOfFrame();
        InitGame();
    }

    public static Dictionary<CropInformation, List<Crop>> Crops { get; } = new Dictionary<CropInformation, List<Crop>>();
    public static Dictionary<CropInformation, List<CropData>> CropDatas { get; } = new Dictionary<CropInformation, List<CropData>>();

    public static Dictionary<string, List<Enemy>> Enemies { get; } = new Dictionary<string, List<Enemy>>();
    public static Dictionary<string, EnemyInformation> EnemyInfos { get; } = new Dictionary<string, EnemyInformation>();

    public static Dictionary<string, TalkerInformation> TalkerInfos { get; } = new Dictionary<string, TalkerInformation>();

    public static Dictionary<string, ItemBase> Items { get; } = new Dictionary<string, ItemBase>();

    public static void InitGame(params Type[] exceptions)
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        ActionStack.Clear();
        Crops.Clear();
        foreach (var enemykvp in Enemies)
            enemykvp.Value.RemoveAll(x => !x || !x.gameObject);

        EnemyInfos.Clear();
        var enemies = Resources.LoadAll<EnemyInformation>("Configuration");
        foreach (var e in enemies)
            EnemyInfos.Add(e.ID, e);

        Items.Clear();
        var items = Resources.LoadAll<ItemBase>("Configuration");
        foreach (var i in items)
            Items.Add(i.ID, i);

        TalkerInfos.Clear();
        var talkers = Resources.LoadAll<TalkerInformation>("Configuration");
        foreach (var t in talkers)
            TalkerInfos.Add(t.ID, t);

        if (exceptions == null || !exceptions.Contains(typeof(TriggerHolder)))
            foreach (var tholder in FindObjectsOfType<TriggerHolder>())
                tholder.Init();
        PlayerManager.Instance.Init();
        DialogueManager.Instance.Init();
        if (!UIManager.Instance || !UIManager.Instance.gameObject) Instantiate(Instance.UIPrefab);
        UIManager.Instance.Init();
        FieldManager.Instance.Init();
        QuestManager.Instance.Init();
        MessageManager.Instance.Init();
        MapManager.Instance.Init();
        MapManager.Instance.SetPlayer(PlayerManager.Instance.PlayerTransform);
        MapManager.Instance.RemakeCamera();
        GatherManager.Instance.Init();
        NewWindowsManager.Clear();
    }
}