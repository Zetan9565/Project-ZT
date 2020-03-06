using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    [DisplayName("清理周期(秒)")]
#endif
    private float cleanDelayTime = 600.0f;//池子东西放多放久了臭，要按时排掉

    public void Put(GameObject gameObject)
    {
        if (!gameObject) return;
        ZetanUtility.SetActive(gameObject, false);
        gameObject.transform.SetParent(poolRoot, false);
        string name = gameObject.name.EndsWith("(Clone)") ? gameObject.name : gameObject.name + "Clone";
        pool.TryGetValue(name, out var oListFound);
        if (oListFound != null)
        {
            if (!oListFound.Contains(gameObject)) oListFound.Add(gameObject);
        }
        else
        {
            pool.Add(name, new HashSet<GameObject>() { gameObject });
        }
    }
    public void Put(Component component)
    {
        Put(component.gameObject);
    }

    public void Put(GameObject gameObject, float delayTime)
    {
        StartCoroutine(PutDelay(gameObject, delayTime));
    }
    public void Put(Component component, float delayTime)
    {
        Put(component.gameObject, delayTime);
    }
    IEnumerator PutDelay(GameObject gameObject, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Put(gameObject);
    }

    public GameObject Get(GameObject prefab, Transform parent = null, bool worldPositionStays = false)
    {
        string goName = prefab.name.EndsWith("(Clone)") ? prefab.name : prefab.name + "(Clone)";
        pool.TryGetValue(goName, out var oListFound);
        if (oListFound != null && oListFound.Count > 0)
        {
            GameObject go = oListFound.ElementAt(0);
            oListFound.Remove(go);
            if (oListFound.Count < 1) pool.Remove(goName);
            go.transform.SetParent(parent, worldPositionStays);
            ZetanUtility.SetActive(go, true);
            return go;
        }
        else
        {
            GameObject go = Instantiate(prefab, parent, worldPositionStays);
            return go;
        }
    }
    public T Get<T>(T prefab, Transform parent = null, bool worldPositionStays = false) where T : Component
    {
        string goName = prefab.name.EndsWith("(Clone)") ? prefab.name : prefab.name + "(Clone)";
        pool.TryGetValue(goName, out var oListFound);
        if (oListFound != null && oListFound.Count > 0)
        {
            GameObject go = oListFound.ElementAt(0);
            oListFound.Remove(go);
            if (oListFound.Count < 1) pool.Remove(goName);
            go.transform.SetParent(parent, worldPositionStays);
            ZetanUtility.SetActive(go, true);
            return go.GetComponent<T>();
        }
        else
        {
            GameObject go = Instantiate(prefab.gameObject, parent, worldPositionStays);
            return go.GetComponent<T>();
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool worldPositionStays = false)
    {
        string goName = prefab.name.EndsWith("(Clone)") ? prefab.name : prefab.name + "(Clone)";
        pool.TryGetValue(goName, out var oListFound);
        if (oListFound != null && oListFound.Count > 0)
        {
            GameObject go = oListFound.ElementAt(0);
            go.transform.position = position;
            go.transform.rotation = rotation;
            oListFound.Remove(go);
            if (oListFound.Count < 1) pool.Remove(goName);
            go.transform.SetParent(parent, worldPositionStays);
            ZetanUtility.SetActive(go, true);
            return go;
        }
        else
        {
            GameObject go = Instantiate(prefab, position, rotation);
            if (parent) go.transform.SetParent(parent, worldPositionStays);
            return go;
        }
    }
    public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool worldPositionStays = false) where T : Component
    {
        string goName = prefab.name.EndsWith("(Clone)") ? prefab.name : prefab.name + "(Clone)";
        pool.TryGetValue(goName, out var oListFound);
        if (oListFound != null && oListFound.Count > 0)
        {
            GameObject go = oListFound.ElementAt(0);
            go.transform.position = position;
            go.transform.rotation = rotation;
            oListFound.Remove(go);
            if (oListFound.Count < 1) pool.Remove(goName);
            go.transform.SetParent(parent, worldPositionStays);
            ZetanUtility.SetActive(go, true);
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
            {
                /*for (int j = pool[keys[i]].Count - 1; j >= 0; j--)
                {
                    Destroy(pool[keys[i]][j]);
                }*/
                foreach (var o in pool[keys[i]])
                {
                    Destroy(o);
                }
            }
            pool.Clear();
            System.GC.Collect();
        }
    }
}