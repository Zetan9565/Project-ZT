using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "dialogue", menuName = "ZetanStudio/剧情/对话")]
public class Dialogue : ScriptableObject
{
    [SerializeField]
#if UNITY_EDITOR
    [ReadOnly]
#endif
    private List<DialogueWords> words = new List<DialogueWords>();
    public List<DialogueWords> Words
    {
        get
        {
            return words;
        }
    }
}
[System.Serializable]
public class DialogueWords
{
    public string TalkerName
    {
        get
        {
            if (TalkerInfo)
                return TalkerInfo.Name;
            else return string.Empty;
        }
    }

    [SerializeField]
    private NPCInfomation talkerInfo;
    public NPCInfomation TalkerInfo
    {
        get
        {
            return talkerInfo;
        }
    }

    [SerializeField, TextArea(3, 10)]
    private string words;
    public string Words
    {
        get
        {
            return words;
        }
    }

}