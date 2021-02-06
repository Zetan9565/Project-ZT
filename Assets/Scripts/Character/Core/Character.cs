using System.Collections;
using UnityEngine;

public class Character : ManagedObject
{
    [SerializeField]
    protected CharacterInformation info;
    public CharacterInformation Info => info;

    protected CharacterData data;
    public CharacterData Data { get => data; protected set => data = value; }

    public override bool Init()
    {
        Data = new CharacterData(Info);
        Data.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Data.currentPosition = transform.position;
        return base.Init();
    }

    public void SetInfo(CharacterInformation info)
    {
        Data = new CharacterData(info);
        Data.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Data.currentPosition = transform.position;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}