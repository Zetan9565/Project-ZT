﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class ObjectPool : SingletonMonoBehaviour<ObjectPool>
{
    private readonly Dictionary<string, HashSet<GameObject>> pool = new Dictionary<string, HashSet<GameObject>>();
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("池子")]
#endif
    private Transform poolRoot;//用于放置失效对象

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("容量")]
#endif
    private int capacity = 500;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("清理周期(秒)")]
#endif
    private float cleanDelayTime = 600.0f;//池子东西放多放久了臭，要按时排掉

    private readonly Dictionary<GameObject, string> putLog = new Dictionary<GameObject, string>();

    public static void Put(GameObject gameObject)
    {
        if (!gameObject) return;
        if (!Instance || Instance.pool.Count > Instance.capacity)
        {
            Destroy(gameObject);
            return;
        }
        if (Instance.putDelayCoroutines.TryGetValue(gameObject, out var c)) Instance.StopCoroutine(c);
        ZetanUtility.SetActive(gameObject, false);
        gameObject.transform.SetParent(Instance.poolRoot, false);
        string name = gameObject.name.EndsWith("(Clone)") ? gameObject.name : gameObject.name + "Clone";
        if (Instance.pool.TryGetValue(name, out var oListFound))
        {
            StackTrace st = new StackTrace(true);
            if (!oListFound.Contains(gameObject))
            {
                var sf = st.GetFrame(1);
                if (sf != null)
                {
                    if (Instance.putLog.ContainsKey(gameObject)) Instance.putLog[gameObject] = $"{sf.GetFileName()}:{sf.GetFileLineNumber()}";
                    else Instance.putLog.Add(gameObject, $"{sf.GetFileName()}:{sf.GetFileLineNumber()}");
                }
                oListFound.Add(gameObject);
            }
            else
            {
                var sf = st.GetFrame(1);
                UnityEngine.Debug.LogError($"重复入池：上次：{Instance.putLog[gameObject]}\n当前：{sf.GetFileName()}:{sf.GetFileLineNumber()}");
            }
        }
        else
        {
            Instance.pool.Add(name, new HashSet<GameObject>() { gameObject });
        }
    }
    public static void Put(Component component)
    {
        Put(component.gameObject);
    }

    private readonly Dictionary<GameObject, Coroutine> putDelayCoroutines = new Dictionary<GameObject, Coroutine>();
    public static void Put(GameObject gameObject, float delayTime)
    {
        if (!Instance || Instance.pool.Count > Instance.capacity)
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
        if (!Instance) return Instantiate(prefab, parent, worldPositionStays);
        string goName = prefab.name.EndsWith("(Clone)") ? prefab.name : prefab.name + "(Clone)";
        if (Instance.pool.TryGetValue(goName, out var oListFound) && oListFound.Count > 0)
        {
            GameObject go = oListFound.ElementAt(0);
            oListFound.Remove(go);
            if (oListFound.Count < 1) Instance.pool.Remove(goName);
            go.transform.SetParent(parent, worldPositionStays);
            ZetanUtility.SetActive(go, true);
            if (Instance.putDelayCoroutines.TryGetValue(go, out var c))
            {
                Instance.StopCoroutine(c);
                Instance.putDelayCoroutines.Remove(go);
            }
            return go;
        }
        else
        {
            return Instantiate(prefab, parent, worldPositionStays);
        }
    }
    public static T Get<T>(T prefab, Transform parent = null, bool worldPositionStays = false) where T : Component
    {
        if (!Instance) return Instantiate(prefab.gameObject, parent, worldPositionStays).GetComponent<T>();
        string goName = prefab.name.EndsWith("(Clone)") ? prefab.name : prefab.name + "(Clone)";
        Instance.pool.TryGetValue(goName, out var oListFound);
        if (oListFound != null && oListFound.Count > 0)
        {
            GameObject go = oListFound.ElementAt(0);
            oListFound.Remove(go);
            if (oListFound.Count < 1) Instance.pool.Remove(goName);
            go.transform.SetParent(parent, worldPositionStays);
            ZetanUtility.SetActive(go, true);
            if (Instance.putDelayCoroutines.TryGetValue(go, out var c))
            {
                Instance.StopCoroutine(c);
                Instance.putDelayCoroutines.Remove(go);
            }
            return go.GetComponent<T>();
        }
        else
        {
            GameObject go = Instantiate(prefab.gameObject, parent, worldPositionStays);
            return go.GetComponent<T>();
        }
    }

    public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool worldPositionStays = false)
    {
        if (!Instance)
        {
            GameObject go = Instantiate(prefab, position, rotation);
            if (parent) go.transform.SetParent(parent, worldPositionStays);
            return go;
        }
        string goName = prefab.name.EndsWith("(Clone)") ? prefab.name : prefab.name + "(Clone)";
        Instance.pool.TryGetValue(goName, out var oListFound);
        if (oListFound != null && oListFound.Count > 0)
        {
            GameObject go = oListFound.ElementAt(0);
            go.transform.position = position;
            go.transform.rotation = rotation;
            oListFound.Remove(go);
            if (oListFound.Count < 1) Instance.pool.Remove(goName);
            go.transform.SetParent(parent, worldPositionStays);
            ZetanUtility.SetActive(go, true);
            if (Instance.putDelayCoroutines.TryGetValue(go, out var c))
            {
                Instance.StopCoroutine(c);
                Instance.putDelayCoroutines.Remove(go);
            }
            return go;
        }
        else
        {
            GameObject go = Instantiate(prefab, position, rotation);
            if (parent) go.transform.SetParent(parent, worldPositionStays);
            return go;
        }
    }
    public static T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool worldPositionStays = false) where T : Component
    {
        if (!Instance)
        {
            GameObject go = Instantiate(prefab.gameObject, position, rotation);
            if (parent) go.transform.SetParent(parent, worldPositionStays);
            return go.GetComponent<T>();
        }
        string goName = prefab.name.EndsWith("(Clone)") ? prefab.name : prefab.name + "(Clone)";
        Instance.pool.TryGetValue(goName, out var oListFound);
        if (oListFound != null && oListFound.Count > 0)
        {
            GameObject go = oListFound.ElementAt(0);
            go.transform.position = position;
            go.transform.rotation = rotation;
            oListFound.Remove(go);
            if (oListFound.Count < 1) Instance.pool.Remove(goName);
            go.transform.SetParent(parent, worldPositionStays);
            ZetanUtility.SetActive(go, true);
            if (Instance.putDelayCoroutines.TryGetValue(go, out var c))
            {
                Instance.StopCoroutine(c);
                Instance.putDelayCoroutines.Remove(go);
            }
            return go.GetComponent<T>();
        }
        else
        {
            GameObject go = Instantiate(prefab.gameObject, position, rotation);
            if (parent) go.transform.SetParent(parent, worldPositionStays);
            return go.GetComponent<T>();
        }
    }

    private void Awake()
    {
        if (!poolRoot) poolRoot = transform;
        StartCoroutine(CleanDelay());
    }

    private IEnumerator CleanDelay()
    {
        yield return new WaitForSeconds(cleanDelayTime);
        Clean();
        StartCoroutine(CleanDelay());
    }

    public void Clean()
    {
        if (pool.Count > 0)
        {
            List<string> keys = new List<string>(pool.Keys);
            for (int i = 0; i < keys.Count; i++)
                foreach (var o in pool[keys[i]])
                    if (o) Destroy(o);
            pool.Clear();
            putLog.Clear();
            System.GC.Collect();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("ZetanStudio/添加对象池")]
    private static void MakePool()
    {
        if (Instance)
        {
            UnityEngine.Debug.Log("已存在对象池");
            return;
        }
        new GameObject("ObjectPool").AddComponent<ObjectPool>();
    }
#endif
}