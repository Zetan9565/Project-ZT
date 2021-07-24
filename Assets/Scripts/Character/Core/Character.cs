using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour, IManageAble
{
    [SerializeField]
    protected CharacterInformation info;
    public CharacterInformation Info => info;

    protected CharacterData data;
    public CharacterData Data { get => data; protected set => data = value; }

    public bool IsInit { get; protected set; }

    public virtual bool Init()
    {
        Data = new CharacterData(Info);
        Data.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Data.currentPosition = transform.position;
        IsInit = true;
        return true;
    }

    public void SetInfo(CharacterInformation info)
    {
        Data = new CharacterData(info);
        Data.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Data.currentPosition = transform.position;
    }

    public bool Reset()
    {
        throw new System.NotImplementedException();
    }

    public bool OnSaveGame(SaveData data)
    {
        throw new System.NotImplementedException();
    }

    public bool OnLoadGame(SaveData data)
    {
        throw new System.NotImplementedException();
    }
}