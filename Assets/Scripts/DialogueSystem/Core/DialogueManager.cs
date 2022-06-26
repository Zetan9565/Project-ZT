using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZetanStudio.Extension;

public delegate void DialogueListner();
public static class DialogueManager
{
    private static Transform talkerRoot;

    private static readonly Dictionary<string, DialogueData> dialogueDatas = new Dictionary<string, DialogueData>();

    public static Dictionary<string, TalkerData> Talkers { get; } = new Dictionary<string, TalkerData>();
    private readonly static Dictionary<string, List<TalkerData>> scenedTalkers = new Dictionary<string, List<TalkerData>>();

    [InitMethod(-1)]
    public static void Init()
    {
        if (!talkerRoot || !talkerRoot.gameObject)
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

    public static DialogueData GetOrCreateDialogueData(Dialogue dialogue)
    {
        if (!dialogueDatas.TryGetValue(dialogue.ID, out var find))
        {
            find = new DialogueData(dialogue);
            dialogueDatas.Add(dialogue.ID, find);
        }
        return find;
    }

    public static void RemoveDialogueData(Dialogue dialogue)
    {
        if (!dialogue) return;
        dialogueDatas.Remove(dialogue.ID);
    }

    [SaveMethod]
    public static void SaveData(SaveData saveData)
    {

        var dialog = new SaveDataItem();
        saveData.data["dialogueData"] = dialog;
        foreach (var d in dialogueDatas.Values)
        {
            var ds = new SaveDataItem();
            ds.stringData["dialogID"] = d.ID;
            foreach (var w in d.wordsDatas)
            {
                var ws = new SaveDataItem();
                for (int i = 0; i < w.OptionDatas.Count; i++)
                {
                    if (w.OptionDatas[i].isDone)
                        ws.intList.Add(i);
                }
                ds.dataList.Add(ws);
            }
            dialog.subData[d.ID] = ds;
        }
    }

    [LoadMethod]
    public static void LoadData(SaveData saveData)
    {
        dialogueDatas.Clear();
        if (saveData.data.TryGetValue("dialogueData", out var data))
        {
            Dictionary<string, Dialogue> dialogues = new Dictionary<string, Dialogue>();
            foreach (var dialog in Resources.LoadAll<Dialogue>("Configuration"))
            {
                dialogues[dialog.ID] = dialog;
            }
            foreach (var ds in data.subData)
            {
                if (dialogues.TryGetValue(ds.Key, out var find))
                {
                    DialogueData dd = new DialogueData(find);
                    for (int i = 0; i < dd.wordsDatas.Count; i++)
                    {
                        try
                        {
                            var cmplt = new HashSet<int>(ds.Value.dataList[i].intList);
                            for (int j = 0; j < dd.wordsDatas[i].OptionDatas.Count; j++)
                            {
                                if (cmplt.Contains(j)) dd.wordsDatas[i].OptionDatas[j].isDone = true;
                            }
                        }
                        catch { }
                    }
                    dialogueDatas.Add(dd.ID, dd);
                }
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