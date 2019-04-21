using UnityEngine;

[CreateAssetMenu(fileName = "quest group", menuName = "ZetanStudio/任务/任务组")]
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
    private string _Name;
    public string Name
    {
        get
        {
            return _Name;
        }
    }
}