using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZetanStudio.Item
{
    [CreateAssetMenu]
    public sealed class ItemDatabase : SingletonScriptableObject<ItemDatabase>
    {
        [SerializeField]
        private List<ItemNew> items = new List<ItemNew>();

        public static Dictionary<string, ItemNew> ToDictionary()
        {
            if (Instance) return Instance.items.ToDictionary(x => x.ID);
            else return null;
        }

#if UNITY_EDITOR
        public static class Editor
        {
            public static List<ItemNew> GetItems()
            {
                return new List<ItemNew>(GetOrCreate().items);
            }
            public static ItemNew MakeItem(ItemTemplate template)
            {
                var instance = GetOrCreate();
                ItemNew item = CreateInstance<ItemNew>();
                ItemNew.Editor.ApplyTemplate(item, template);
                ItemNew.Editor.SetAutoID(item, instance.items, template ? template.IDPrefix : null);
                instance.items.Add(item);
                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssetIfDirty(item);
                item.name = "item";
                AssetDatabase.AddObjectToAsset(item, instance);
                EditorUtility.SetDirty(instance);
                AssetDatabase.SaveAssetIfDirty(instance);
                return item;
            }
            public static bool DeleteItem(ItemNew item)
            {
                if (!item || !instance) return false;
                if (!Instance.items.Remove(item)) return false;
                AssetDatabase.RemoveObjectFromAsset(item);
                EditorUtility.SetDirty(instance);
                AssetDatabase.SaveAssetIfDirty(instance);
                return true;
            }
        }
#endif
    }
}