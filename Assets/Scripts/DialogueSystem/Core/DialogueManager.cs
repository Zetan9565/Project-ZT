using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    using CharacterSystem;
    using Extension;
    using SavingSystem;

    public static class DialogueManager
    {
        private static Transform talkerRoot;

        private static readonly Dictionary<string, DialogueData> data = new Dictionary<string, DialogueData>();

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
                if (ti.Scene == Utility.GetActiveScene().name)
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

        public static DialogueData GetOrCreateData(EntryContent entry)
        {
            if (!entry) return null;
            if (!data.TryGetValue(entry.ID, out var find))
                data.Add(entry.ID, find = new DialogueData(entry));
            else find.Refresh(entry);
            return find;
        }

        public static void RemoveData(EntryContent entry)
        {
            if (!entry) return;
            data.Remove(entry.ID);
        }

        [SaveMethod]
        public static void SaveData(SaveData saveData)
        {

            var dialog = new GenericData();
            saveData["dialogueData"] = dialog;
            foreach (var d in data.Values)
            {
                dialog[d.ID] = d.GetSaveData();
            }
        }

        [LoadMethod]
        public static void LoadData(SaveData saveData)
        {
            data.Clear();
            if (saveData.TryReadData("dialogueData", out var dialog))
                foreach (var kvp in dialog.ReadDataDict())
                {
                    data[kvp.Key] = new DialogueData(kvp.Value);
                }
        }
    }
}