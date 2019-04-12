using UnityEngine;

public class CharacterInfomation : ScriptableObject
{
    [SerializeField]
    protected string _ID;
    public string ID
    {
        get
        {
            return _ID;
        }
    }

    [SerializeField]
    protected string _Name;
    public string Name
    {
        get
        {
            return _Name;
        }
    }
}
