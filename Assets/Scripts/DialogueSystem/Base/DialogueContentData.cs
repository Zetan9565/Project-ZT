using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ZetanStudio.DialogueSystem
{
    public sealed class DialogueContentData
    {
        public DialogueContentData this[DialogueContent content] => family.TryGetValue(content.ID, out var find) ? find : null;
        public DialogueContentData this[string id] => family.TryGetValue(id, out var find) ? find : null;

        public readonly string ID;
        private bool said;
        private bool exitHere;
        private bool returnable;

        public bool IsDone => said && (children.Count < 1 || Traverse(this, x => x.exitHere && x.IsDone) || children.All(x => x.returnable && x.IsDone));

        private readonly List<DialogueContentData> children = new List<DialogueContentData>();
        public ReadOnlyCollection<DialogueContentData> Children => new ReadOnlyCollection<DialogueContentData>(children);

        private readonly Dictionary<string, DialogueContentData> family = new Dictionary<string, DialogueContentData>();
        public ReadOnlyDictionary<string, DialogueContentData> Family => new ReadOnlyDictionary<string, DialogueContentData>(family);

        public void Access() => said = true;

        public void Refresh(EntryContent entry)
        {
            if (entry?.ID != ID) return;
            Dialogue.Traverse(entry, c =>
            {
                if (!family.ContainsKey(c.ID)) family[c.ID] = new DialogueContentData(c, family);
                family[c.ID].exitHere = c.ExitHere;
                family[c.ID].returnable = Dialogue.Traverse(c, c => c.Options.All(x => x.Content is RecursionSuffix));
            });
            var invalid = family.Where(x => !Dialogue.Traverse(entry, n => n.ID == x.Key));
            foreach (var kvp in invalid)
            {
                family.Remove(kvp.Key);
            }
            removeUnused(this);

            void removeUnused(DialogueContentData data)
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

        public DialogueContentData(SaveDataItem saveData)
        {
            saveData.TryReadString("ID", out ID);
            saveData.TryReadBool("said", out said);
            if (saveData.TryReadData("family", out var cts))
                foreach (var ct in cts.ReadDataDict())
                {
                    var dcd = family[ct.Key] = new DialogueContentData(ct.Key, family);
                    ct.Value.TryReadBool("said", out dcd.said);
                    ct.Value.TryReadBool("exitable", out dcd.exitHere);
                    ct.Value.TryReadBool("returnable", out dcd.returnable);
                }
            if (saveData.TryReadData("children", out var cdn))
                foreach (var cd in cdn.ReadDataList())
                {
                    loadChild(this, cd);
                }
            family[ID] = this;

            void loadChild(DialogueContentData content, SaveDataItem cd)
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
        public DialogueContentData(EntryContent entry) : this(entry, new Dictionary<string, DialogueContentData>()) { }
        private DialogueContentData(string ID, Dictionary<string, DialogueContentData> family)
        {
            this.ID = ID;
            this.family = family;
        }

        private DialogueContentData(DialogueContent content, Dictionary<string, DialogueContentData> family)
        {
            ID = content.ID;
            exitHere = content.ExitHere;
            returnable = Dialogue.Traverse(content, c => c.Options.All(x => x.Content is RecursionSuffix));
            if (!exitHere)
                foreach (var option in content.Options)
                {
                    if (option.Content)
                        if (!family.TryGetValue(option.Content.ID, out var find))
                            children.Add(family[option.Content.ID] = new DialogueContentData(option.Content, family));
                        else children.Add(find);
                }
            family[ID] = this;
            this.family = family;
        }

        public static void Traverse(DialogueContentData data, Action<DialogueContentData> onAccess)
        {
            if (data != null)
            {
                onAccess?.Invoke(data);
                data.children.ForEach(c => Traverse(c, onAccess));
            }
        }

        ///<param name="onAccess">带中止条件的访问器，返回 <i>true</i> 时将中止遍历</param>
        /// <returns>是否在遍历时产生中止</returns>
        public static bool Traverse(DialogueContentData data, Func<DialogueContentData, bool> onAccess)
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

        public static implicit operator bool(DialogueContentData data) => data != null;

        public SaveDataItem GetSaveData()
        {
            var data = new SaveDataItem();
            data["ID"] = ID;
            data["said"] = said;
            data["exitable"] = exitHere;
            data["returnable"] = returnable;
            if (children.Count > 0)
            {
                var cdn = new SaveDataItem();
                foreach (var child in children)
                {
                    cdn.Write(makeChild(child));
                }
                data["children"] = cdn;
            }
            if (family.Count > 0)
            {
                var cts = new SaveDataItem();
                foreach (var content in family)
                {
                    var cd = new SaveDataItem();
                    cts[content.Key] = cd;
                    cd["ID"] = content.Key;
                    cd["said"] = content.Value.said;
                }
                data["family"] = cts;
            }
            return data;

            static SaveDataItem makeChild(DialogueContentData child)
            {
                var cd = new SaveDataItem();
                cd["ID"] = child.ID;
                if (child.children.Count > 0)
                {
                    var cdn = new SaveDataItem();
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