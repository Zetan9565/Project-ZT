using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ZetanStudio.Extension;

public delegate void DialogueListner();
public class DialogueManager : SingletonMonoBehaviour<DialogueManager>, ISaveLoad
{
    private Transform talkerRoot;

    private readonly Dictionary<string, DialogueData> dialogueDatas = new Dictionary<string, DialogueData>();

    public Dictionary<string, TalkerData> Talkers { get; } = new Dictionary<string, TalkerData>();
    private readonly Dictionary<string, List<TalkerData>> scenedTalkers = new Dictionary<string, List<TalkerData>>();

    public void Init()
    {
        if (!talkerRoot)
            talkerRoot = new GameObject("Talkers").transform;
        Talkers.Clear();
        scenedTalkers.Clear();
        foreach (var ti in Resources.LoadAll<TalkerInformation>("Configuration").Where(x => x.IsValid && x.Enable))
        {
            TalkerData data = new TalkerData(ti);
            if (ti.Scene == ZetanUtility.ActiveScene.name)
            {
                Talker talker = ti.Prefab.Instantiate(talkerRoot).GetComponent<Talker>();
                talker.Init(data);
            }
            Talkers.Add(ti.ID, data);
            if (scenedTalkers.TryGetValue(ti.Scene, out var talkers))
                talkers.Add(data);
            else scenedTalkers.Add(ti.Scene, new List<TalkerData>() { data });
        }
    }

    public DialogueData GetOrCreateDialogueData(Dialogue dialogue)
    {
        if(!dialogueDatas.TryGetValue(dialogue.ID, out var find))
        {
            find = new DialogueData(dialogue);
            dialogueDatas.Add(dialogue.ID, find);
        }
        return find;
    }

    public void RemoveDialogueData(Dialogue dialogue)
    {
        if (!dialogue) return;
        dialogueDatas.Remove(dialogue.ID);
    }

    public void SaveData(SaveData data)
    {
        foreach (KeyValuePair<string, DialogueData> kvpDialog in dialogueDatas)
        {
            data.dialogueDatas.Add(new DialogueSaveData(kvpDialog.Value));
        }
    }
    public void LoadData(SaveData data)
    {
        dialogueDatas.Clear();
        Dialogue[] dialogues = Resources.LoadAll<Dialogue>("Configuration");
        foreach (DialogueSaveData dsd in data.dialogueDatas)
        {
            Dialogue find = dialogues.FirstOrDefault(x => x.ID == dsd.dialogID);
            if (find)
            {
                DialogueData dd = new DialogueData(find);
                for (int i = 0; i < dsd.wordsDatas.Count; i++)
                {
                    for (int j = 0; j < dd.wordsDatas[i].optionDatas.Count; j++)
                    {
                        if (dsd.wordsDatas[i].IsOptionCmplt(j))
                            dd.wordsDatas[i].optionDatas[j].isDone = true;
                    }
                }
                dialogueDatas.Add(dsd.dialogID, dd);
            }
        }
    }
}

public enum DialogueType
{
    Normal,
    Quest,
    Objective,
    Gift,
}