using UnityEngine;

[CreateAssetMenu(fileName = "quest group", menuName = "Zetan Studio/任务/任务组", order = 2)]
public class QuestGroup : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID
    {
        get
        {
            return _ID;
        }
    }

    [SerializeField]
    private string _name;
    public new string name
    {
        get
        {
            return _name;
        }
    }
}