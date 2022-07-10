using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ZetanStudio
{
    [CreateAssetMenu(fileName = "language", menuName = "Zetan Studio/本地化/语言映射表")]
    public class LanguageMap : ScriptableObject
    {
        [SerializeField]
        private string _name = "zh_hans";
        public string Name => _name;

        [SerializeField]
        protected List<LanguageMapItem> items = new List<LanguageMapItem>();
        public ReadOnlyCollection<LanguageMapItem> Items => new ReadOnlyCollection<LanguageMapItem>(items);

        public Dictionary<string, string> AsDictionary()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var item in items)
            {
                result[item.Key] = item.Value;
            }
            return result;
        }

        public string Tr(string text)
        {
            var item = items.Find(x => x.Key == text);
            if (item != null) return item.Value;
            else
            {
                var match = Regex.Match(text, @"(?<=^<color=[\w]*>)(.*)(?=</color>$)");
                if (match.Success) return text.Replace(match.Value, Tr(match.Value));
            }
            return text;
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
