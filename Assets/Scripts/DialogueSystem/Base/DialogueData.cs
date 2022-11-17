using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ZetanStudio.DialogueSystem
{
    public sealed class DialogueData
    {
        public DialogueData this[DialogueContent content] => family.TryGetValue(content.ID, out var find) ? find : null;
        public DialogueData this[string id] => family.TryGetValue(id, out var find) ? find : null;

        public readonly string ID;
        private bool accessed;
        public bool Accessed => accessed;
        private bool exitHere;
        private bool recursive;

        public GenericData AdditionalData { get; private set; } = new GenericData();

        public bool IsDone => accessed && (exitHere || children.Count < 1 || Traverse(this, x => x != this && x.exitHere && x.IsDone) || children.All(x => x.recursive && x.IsDone));

        private readonly List<DialogueData> children = new List<DialogueData>();
        public ReadOnlyCollection<DialogueData> Children => new ReadOnlyCollection<DialogueData>(children);

        private readonly Dictionary<string, bool> eventStates = new Dictionary<string, bool>();
        public ReadOnlyDictionary<string, bool> EventStates => new ReadOnlyDictionary<string, bool>(eventStates);

        private readonly Dictionary<string, DialogueData> family = new Dictionary<string, DialogueData>();
        public ReadOnlyDictionary<string, DialogueData> Family => new ReadOnlyDictionary<string, DialogueData>(family);

        public void Access() => accessed = true;

        public void AccessEvent(string eventID)
        {
            if (eventStates.ContainsKey(eventID)) eventStates[eventID] = true;
        }

        public void Refresh(EntryContent entry)
        {
            if (entry?.ID != ID) return;
            Dialogue.Traverse(entry, c =>
            {
                DialogueData data = null;
                if (!family.ContainsKey(c.ID)) data = family[c.ID] = new DialogueData(c, family);
                else data = family[c.ID];
                data.exitHere = c.ExitHere;
                data.recursive = Dialogue.Traverse(c, c => c.Options.All(x => x.Content is RecursionSuffix));
                var keys = data.eventStates.Keys.Cast<string>();
                var IDs = c.Events.Select(x => x.ID).ToHashSet();
                foreach (var key in keys)
                {
                    if (!IDs.Contains(key)) data.eventStates.Remove(key);
                }
                foreach (var evt in c.Events)
                {
                    if (evt != null && !string.IsNullOrEmpty(evt.ID))
                        if (!data.eventStates.ContainsKey(evt.ID))
                            data.eventStates[evt.ID] = false;
                }
            });
            var invalid = family.Keys.Where(k => !Dialogue.Traverse(entry, n => n.ID == k)).Cast<string>();
            foreach (var key in invalid)
            {
                family.Remove(key);
            }
            removeUnused(this);

            void removeUnused(DialogueData data)
            {
                for (int i = 0; i < data.children.Count; i++)
                {
                    if (!family.ContainsKey(data.children[i].ID))
                    {
                        data.children.RemoveAt(i);
                        i--;
                    }
                    else removeUnused(data.children[i]);
                }
            }
        }

        public DialogueData(GenericData data)
        {
            data.TryReadString("ID", out ID);
            data.TryReadBool("accessed", out accessed);
            if (data.TryReadData("family", out var cts))
                foreach (var ct in cts.ReadDataDict())
                {
                    var dcd = family[ct.Key] = new DialogueData(ct.Key, family);
                    ct.Value.TryReadBool("accessed", out dcd.accessed);
                    dcd.AdditionalData = ct.Value.ReadData("additional") ?? new GenericData();
                }
            if (data.TryReadData("children", out var cdn))
                foreach (var cd in cdn.ReadDataList())
                {
                    loadChild(this, cd);
                }
            if (data.TryReadData("events", out var es))
                foreach (var kvp in es.ReadBoolDict())
                {
                    eventStates[kvp.Key] = kvp.Value;
                }
            family[ID] = this;
            AdditionalData = data.ReadData("additional") ?? new GenericData();

            void loadChild(DialogueData content, GenericData cd)
            {
                if (family.TryGetValue(cd.ReadString("ID"), out var find))
                {
                    content.children.Add(find);
                    if (cd.TryReadData("children", out var children))
                        foreach (var c in children.ReadDataList())
                        {
                            loadChild(find, c);
                        }
                }
            }
        }
        public DialogueData(EntryContent entry) : this(entry, new Dictionary<string, DialogueData>()) { }
        private DialogueData(string ID, Dictionary<string, DialogueData> family)
        {
            this.ID = ID;
            this.family = family;
        }

        private DialogueData(DialogueContent content, Dictionary<string, DialogueData> family)
        {
            ID = content.ID;
            exitHere = content.ExitHere;
            recursive = Dialogue.Traverse(content, c => c.Options.All(x => x.Content is RecursionSuffix));
            if (!exitHere)
                foreach (var option in content.Options)
                {
                    if (option.Content)
                        if (!family.TryGetValue(option.Content.ID, out var find))
                            children.Add(family[option.Content.ID] = new DialogueData(option.Content, family));
                        else children.Add(find);
                }
            if (content is not INonEvent)
                foreach (var evt in content.Events)
                {
                    if (evt != null && !string.IsNullOrEmpty(evt.ID))
                        eventStates[evt.ID] = false;
                }
            family[ID] = this;
            this.family = family;
        }

        public static void Traverse(DialogueData data, Action<DialogueData> onAccess)
        {
            if (data != null)
            {
                onAccess?.Invoke(data);
                data.children.ForEach(c => Traverse(c, onAccess));
            }
        }

        ///<param name="onAccess">带中止条件的访问器，返回 <i>true</i> 时将中止遍历</param>
        /// <returns>是否在遍历时产生中止</returns>
        public static bool Traverse(DialogueData data, Func<DialogueData, bool> onAccess)
        {
            if (onAccess != null && data)
            {
                if (onAccess(data)) return true;
                foreach (var child in data.children)
                {
                    if (Traverse(child, onAccess))
                        return true;
                }
            }
            return false;
        }

        public static implicit operator bool(DialogueData data) => data != null;

        public GenericData GetSaveData()
        {
            var data = new GenericData();
            data["ID"] = ID;
            data["accessed"] = accessed;
            data["additional"] = AdditionalData;
            if (children.Count > 0)
            {
                var cdn = new GenericData();
                foreach (var child in children)
                {
                    cdn.Write(makeChild(child));
                }
                data["children"] = cdn;
            }
            if (family.Count > 0)
            {
                var cts = new GenericData();
                foreach (var kvp in family)
                {
                    var cd = new GenericData();
                    cts[kvp.Key] = cd;
                    cd["ID"] = kvp.Key;
                    cd["accessed"] = kvp.Value.accessed;
                    cd["additional"] = kvp.Value.AdditionalData;
                }
                data["family"] = cts;
            }
            if (eventStates.Count > 0)
            {
                var es = new GenericData();
                foreach (var kvp in eventStates)
                {
                    es[kvp.Key] = kvp.Value;
                }
                data["events"] = es;
            }
            return data;

            static GenericData makeChild(DialogueData child)
            {
                var cd = new GenericData();
                cd["ID"] = child.ID;
                if (child.children.Count > 0)
                {
                    var cdn = new GenericData();
                    foreach (var c in child.children)
                    {
                        cdn.Write(makeChild(c));
                    }
                    cd["children"] = cdn;
                }
                return cd;
            }
        }
    }
}