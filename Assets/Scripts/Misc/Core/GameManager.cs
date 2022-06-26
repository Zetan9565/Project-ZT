using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ZetanStudio.ItemSystem;

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
        else DestroyImmediate(gameObject);
    }

    private void OnApplicationQuit()
    {
        IsExiting = true;
        QuitAttribute.QuitAll();
        QuitMethodAttribute.QuitAll();
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
            EnemyInfos[e.ID] = e;

        TalkerInfos.Clear();
        var talkers = Resources.LoadAll<TalkerInformation>("Configuration");
        foreach (var t in talkers)
            TalkerInfos[t.ID] = t;

        if (exceptions == null || !exceptions.Contains(typeof(TriggerHolder)))
            foreach (var tholder in FindObjectsOfType<TriggerHolder>())
                tholder.Init();
        PlayerManager.Instance.Init();
        if (!UIManager.Instance || !UIManager.Instance.gameObject) Instantiate(Instance.UIPrefab);
        UIManager.Instance.Init();
        FieldManager.Init();
        //QuestManager.Init();
        MessageManager.Instance.Init();
        MapManager.Instance.Init();
        MapManager.Instance.SetPlayer(PlayerManager.Instance.PlayerTransform);
        MapManager.Instance.RemakeCamera();
        //GatherManager.Init();
        //WindowsManager.Init();
        InitAttribute.InitAll();
        InitMethodAttribute.InitAll();
    }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class InitAttribute : Attribute
{
    public readonly string method;
    public readonly int priority;

    public InitAttribute(string method, int priority = 0)
    {
        this.method = method;
        this.priority = priority;
    }

    public static void InitAll()
    {
        var types = new List<Type>(ZetanUtility.GetTypesWithAttribute<InitAttribute>());
        types.Sort((x, y) =>
        {
            var attrx = x.GetCustomAttribute<InitAttribute>();
            var attry = y.GetCustomAttribute<InitAttribute>();
            if (attrx.priority < attry.priority)
                return -1;
            else if (attrx.priority > attry.priority)
                return 1;
            return 0;
        });
        foreach (var type in types)
        {
            try
            {
                type.GetMethod(type.GetCustomAttribute<InitAttribute>().method, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Invoke(null, null);
            }
            catch { }
        }
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class InitMethodAttribute : Attribute
{
    public readonly int priority;

    public InitMethodAttribute(int priority = 0)
    {
        this.priority = priority;
    }

    public static void InitAll()
    {
        var methods = new List<MethodInfo>(ZetanUtility.GetMethodsWithAttribute<InitMethodAttribute>());
        methods.Sort((x, y) =>
        {
            var attrx = x.GetCustomAttribute<InitMethodAttribute>();
            var attry = y.GetCustomAttribute<InitMethodAttribute>();
            if (attrx.priority < attry.priority)
                return -1;
            else if (attrx.priority > attry.priority)
                return 1;
            return 0;
        });
        foreach (var method in methods)
        {
            try
            {
                method.Invoke(null, null);
            }
            catch { }
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class QuitAttribute : Attribute
{
    public readonly string method;

    public QuitAttribute(string method)
    {
        this.method = method;
    }

    public static void QuitAll()
    {
        foreach (var type in ZetanUtility.GetTypesWithAttribute<QuitAttribute>())
        {
            try
            {
                type.GetMethod(type.GetCustomAttribute<InitAttribute>().method, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Invoke(null, null);
            }
            catch { }
        }
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class QuitMethodAttribute : Attribute
{
    public readonly int priority;

    public QuitMethodAttribute(int priority = 0)
    {
        this.priority = priority;
    }

    public static void QuitAll()
    {
        var methods = new List<MethodInfo>(ZetanUtility.GetMethodsWithAttribute<QuitMethodAttribute>());
        methods.Sort((x, y) =>
        {
            var attrx = x.GetCustomAttribute<QuitMethodAttribute>();
            var attry = y.GetCustomAttribute<QuitMethodAttribute>();
            if (attrx.priority < attry.priority)
                return -1;
            else if (attrx.priority > attry.priority)
                return 1;
            return 0;
        });
        foreach (var method in methods)
        {
            try
            {
                method.Invoke(null, null);
            }
            catch { }
        }
    }
}