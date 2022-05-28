using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ZetanStudio
{
    [CreateAssetMenu(fileName = "language", menuName = "Zetan Studio/语言映射表")]
    public class LanguageMap : ScriptableObject
    {
        [SerializeField]
        protected List<LanguageMapItem> items = new List<LanguageMapItem>();
        public ReadOnlyCollection<LanguageMapItem> Items => new ReadOnlyCollection<LanguageMapItem>(items);

        public Dictionary<string, string> AsDictionary()
        {
            return items.ToDictionary(x => x.Key, x => x.Value);
        }

        public string Tr(string text)
        {
            return items.Find(x => x.Key == text)?.Value ?? text;
        }
    }

    [System.Serializable]
    public sealed class LanguageMapItem
    {
        [field: SerializeField, TextArea]
        public string Key { get; private set; }

        [field: SerializeField, TextArea]
        public string Value { get; private set; }

        public LanguageMapItem()
        {

        }

        public LanguageMapItem(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
