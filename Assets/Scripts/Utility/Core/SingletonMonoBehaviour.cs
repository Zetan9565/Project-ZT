using UnityEngine;

public abstract class SingletonMonoBehaviour : MonoBehaviour { }

public abstract class SingletonMonoBehaviour<T> : SingletonMonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<T>(true);
            return instance;
        }
    }
}