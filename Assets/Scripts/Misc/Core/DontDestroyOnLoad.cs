using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class DontDestroyOnLoad : MonoBehaviour
{
    private static bool dontDestroyOnLoadOnce;

    public UnityEvent onAwake;

    void Awake()
    {
        if (!dontDestroyOnLoadOnce)
        {
            DontDestroyOnLoad(gameObject);
            dontDestroyOnLoadOnce = true;
            onAwake?.Invoke();
        }
        else
        {
            Debug.LogError($"重复DontDestroyOnLoad：{gameObject.name}");
            DestroyImmediate(gameObject);
        }
    }
}
