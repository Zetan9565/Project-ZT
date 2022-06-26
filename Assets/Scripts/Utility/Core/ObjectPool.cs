using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : SingletonMonoBehaviour<ObjectPool>
{
    [SerializeField, Label("池子")]
    private Transform poolRoot;//用于放置失效对象
    [SerializeField, Label("容量")]
    private int capacity = 500;
    [SerializeField, Label("清理周期(秒)")]
    private float cleanDelayTime = 600.0f;//池子东西放多放久了臭，要按时排掉
    private bool cleanning;

    private readonly Dictionary<string, ObjectPool<GameObject>> pools = new Dictionary<string, ObjectPool<GameObject>>();
    private readonly Dictionary<GameObject, Coroutine> putDelayCoroutines = new Dictionary<GameObject, Coroutine>();

    public static void Put(GameObject gameObject)
    {
        if (!gameObject) return;
        if (!Instance)
        {
            Destroy(gameObject);
            return;
        }
        if (Instance.putDelayCoroutines.TryGetValue(gameObject, out var c)) Instance.StopCoroutine(c);
        string goName = gameObject.name.EndsWith("(Clone)") ? gameObject.name : gameObject.name + "Clone";
        if (!Instance.pools.TryGetValue(goName, out var pool))
        {
            pool = CreatePool(gameObject);
            Instance.pools.Add(goName, pool);
        }
        pool.Release(gameObject);
    }
    public static void Put(Component component)
    {
        Put(component.gameObject);
    }

    public static void Put(GameObject gameObject, float delayTime)
    {
        if (!gameObject) return;
        if (!Instance)
        {
            Destroy(gameObject, delayTime);
            return;
        }
        if (Instance.putDelayCoroutines.TryGetValue(gameObject, out var c))
        {
            Instance.StopCoroutine(c);
            Instance.putDelayCoroutines[gameObject] = Instance.StartCoroutine(Instance.PutDelay(gameObject, delayTime));
        }
        else Instance.putDelayCoroutines.Add(gameObject, Instance.StartCoroutine(Instance.PutDelay(gameObject, delayTime)));
    }
    public static void Put(Component component, float delayTime)
    {
        Put(component.gameObject, delayTime);
    }
    private IEnumerator PutDelay(GameObject gameObject, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        putDelayCoroutines.Remove(gameObject);
        Put(gameObject);
    }

    public static GameObject Get(GameObject prefab, Transform parent = null, bool worldPositionStays = false)
    {
        GameObject go;
        if (!Instance)
            go = Instantiate(prefab, parent, worldPositionStays);
        else
        {
            string goName = prefab.name.EndsWith("(Clone)") ? prefab.name : prefab.name + "(Clone)";
            if (!Instance.pools.TryGetValue(goName, out var pool))
            {
                pool = CreatePool(prefab);
                Instance.pools.Add(goName, pool);
            }
            go = pool.Get();
            go.transform.SetParent(parent, worldPositionStays);
            if (Instance.putDelayCoroutines.TryGetValue(go, out var c))
            {
                Instance.StopCoroutine(c);
                Instance.putDelayCoroutines.Remove(go);
            }
        }
        return go;
    }
    public static T Get<T>(T prefab, Transform parent = null, bool worldPositionStays = false) where T : Component
    {
        if (!prefab) return null;
        return Get(prefab.gameObject, parent, worldPositionStays).GetComponent<T>();
    }

    public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool worldPositionStays = false)
    {
        GameObject go;
        if (!Instance)
        {
            go = Instantiate(prefab, position, rotation);
            if (parent) go.transform.SetParent(parent, worldPositionStays);
        }
        else
        {
            string goName = prefab.name.EndsWith("(Clone)") ? prefab.name : prefab.name + "(Clone)";
            Instance.pools.TryGetValue(goName, out var pool);
            if (pool != null)
            {
                pool = CreatePool(prefab);
                Instance.pools.Add(goName, pool);
            }
            go = pool.Get();
            go.transform.SetPositionAndRotation(position, rotation);
            go.transform.SetParent(parent, worldPositionStays);
            if (Instance.putDelayCoroutines.TryGetValue(go, out var c))
            {
                Instance.StopCoroutine(c);
                Instance.putDelayCoroutines.Remove(go);
            }
        }
        return go;
    }
    public static T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool worldPositionStays = false) where T : Component
    {
        if (!prefab) return null;
        return Get(prefab.gameObject, position, rotation, parent, worldPositionStays).GetComponent<T>();
    }

    private void Awake()
    {
        if (!poolRoot) poolRoot = transform;
        //StartCoroutine(CleanDelay());
    }

    private IEnumerator CleanDelay()
    {
        yield return new WaitForSeconds(cleanDelayTime);
        cleanning = true;
        Clean();
        cleanning = false;
        StartCoroutine(CleanDelay());
    }

    public void Clean()
    {
        if (pools.Count > 0)
        {
            List<string> keys = new List<string>(pools.Keys);
            for (int i = 0; i < keys.Count; i++)
                pools[keys[i]].Clear();
            pools.Clear();
            System.GC.Collect();
        }
    }

    #region 创建池
    private static ObjectPool<GameObject> CreatePool(GameObject model)
    {
        return new ObjectPool<GameObject>(() => { return Instantiate(model); }, OnGetObject, OnPutObject, OnDestroyObject, true, 10, Instance.capacity);
    }
    private static void OnGetObject(GameObject go)
    {
        ZetanUtility.SetActive(go, true);
    }
    private static void OnPutObject(GameObject go)
    {
        go.transform.SetParent(Instance.poolRoot, false);
        ZetanUtility.SetActive(go, false);
    }
    private static void OnDestroyObject(GameObject go)
    {
        if (!Instance || !Instance.cleanning)
        {
            string name = go.name.EndsWith("(Clone)") ? go.name.Replace("(Clone)", "") : go.name;
            Debug.Log($"因 [{name}] 类型的池子已满，销毁了一个 {name}");
        }
        Destroy(go);
    }
    #endregion

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Zetan Studio/添加对象池")]
    private static void MakePool()
    {
        if (Instance)
        {
            Debug.LogWarning("已存在对象池");
            return;
        }
        new GameObject("ObjectPool", typeof(ObjectPool));
    }
#endif
}
public class SimplePool<T> where T : Component
{
    private readonly Transform poolRoot;

    private readonly ObjectPool<T> pool;

    public T Get(Transform parent = null, bool worldPositionStays = false)
    {
        var go = pool.Get();
        go.transform.SetParent(parent, worldPositionStays);
        return go;
    }
    public T Get(Vector3 position, Quaternion rotation, Transform parent = null, bool worldPositionStays = false)
    {
        var go = pool.Get();
        go.transform.SetPositionAndRotation(position, rotation);
        go.transform.SetParent(parent, worldPositionStays);
        return go;
    }
    public void Put(T element)
    {
        pool.Release(element);
    }
    public void Clear()
    {
        pool.Clear();
    }
    public SimplePool(T model, Transform poolRoot = null, int capacity = 100)
    {
        this.poolRoot = poolRoot;
        pool = new ObjectPool<T>(() => Object.Instantiate(model), OnGetObject, OnPutObject, OnDestroyObject, maxSize: capacity); ;
    }

    private void OnGetObject(T go)
    {
        ZetanUtility.SetActive(go, true);
    }
    private void OnPutObject(T go)
    {
        if (poolRoot) go.transform.SetParent(poolRoot, false);
        ZetanUtility.SetActive(go, false);
    }
    private void OnDestroyObject(T go)
    {
        Object.Destroy(go);
    }
}