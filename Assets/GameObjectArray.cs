using UnityEngine;

public class GameObjectArray : MonoBehaviour
{
    public GameObject[] gameObjects = new GameObject[0];

    public GameObject Get(int index)
    {
        if (gameObjects == null || index < 0 || index >= gameObjects.Length) return null;
        return gameObjects[index];
    }
    public GameObject Get(string name)
    {
        if (gameObjects == null) return null;
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i].name == name) return gameObjects[i];
        }
        return null;
    }
    public GameObject GetWithTag(string tag)
    {
        if (gameObjects == null) return null;
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i].CompareTag(tag)) return gameObjects[i];
        }
        return null;
    }

    public T Get<T>(int index) where T : Component
    {
        if (gameObjects == null || index < 0 || index >= gameObjects.Length) return null;
        return gameObjects[index].GetComponent<T>();
    }

    public void SetActive(int index, bool value)
    {
        if (gameObjects == null || index < 0 || index >= gameObjects.Length) return;
        ZetanUtility.SetActive(gameObjects[index], value);
    }
    public void SetActive(string name, bool value)
    {
        if (gameObjects == null || string.IsNullOrEmpty(name)) return;
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i].name == name)
            {
                ZetanUtility.SetActive(gameObjects[i], value);
                break;
            }
        }
    }

    public bool GetActive(int index)
    {
        var go = Get(index);
        if (go) return go.activeSelf;
        else return false;
    }
    public bool GetActive(string name)
    {
        var go = Get(name);
        if (go) return go.activeSelf;
        else return false;
    }
}
