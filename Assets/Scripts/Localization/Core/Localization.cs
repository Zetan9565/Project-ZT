using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ZetanStudio
{
    [CreateAssetMenu(menuName = "Zetan Studio/本地化/本地化")]
    public sealed class Localization : ScriptableObject
    {
        [field: SerializeField]
        public string Name { get; private set; }

        [SerializeField]
        private List<LanguageData> data = new List<LanguageData>();
        public ReadOnlyCollection<LanguageData> Data => data.AsReadOnly();

        public List<Dictionary<string, string>> FindDictionaries(string selector)
        {
            return data.FindAll(x => x.selectors.Contains(selector)).ConvertAll(x => x.language.AsDictionary());
        }

        public Dictionary<string, List<Dictionary<string, string>>> AsDictionary()
        {
            Dictionary<LanguageMap, Dictionary<string, string>> instances = new Dictionary<LanguageMap, Dictionary<string, string>>();
            Dictionary<string, List<Dictionary<string, string>>> dict = new Dictionary<string, List<Dictionary<string, string>>>();
            foreach (var data in data)
            {
                foreach (var selector in data.selectors)
                {
                    if (!dict.TryGetValue(selector, out var find))
                        dict.Add(selector, new List<Dictionary<string, string>>() { makeDict(data.language) });
                    else find.Add(makeDict(data.language));
                }
            }
            return dict;

            Dictionary<string, string> makeDict(LanguageMap language)
            {
                if (!instances.TryGetValue(language, out var find))
                {
                    find = language.AsDictionary();
                    instances.Add(language, find);
                }
                return find;
            }
        }

        public static string Tr(LanguageMap map, string text)
        {
            if (!map) return text;
            else return map.Tr(text);
        }
        public static string Tr(LanguageMap map, string text, params object[] args)
        {
            return string.Format(Tr(map, text), args);
        }
        public static string[] TrM(LanguageMap map, string text, params string[] texts)
        {
            List<string> results = new List<string>();
            if (!map)
            {
                results.Add(text);
                results.AddRange(texts);
            }
            else
            {
                results.Add(map.Tr(text));
                foreach (var t in texts)
                {
                    results.Add(map.Tr(t));
                }
            }
            return results.ToArray();
        }
        public static string[] TrM(LanguageMap map, string[] texts)
        {
            List<string> results = new List<string>();
            if (!map)
            {
                results.AddRange(texts);
            }
            else
            {
                foreach (var t in texts)
                {
                    results.Add(map.Tr(t));
                }
            }
            return results.ToArray();
        }
    }

    public static class L
    {
        public static string Tr(LanguageMap map, string text)
        {
            return Localization.Tr(map, text);
        }
        public static string Tr(LanguageMap map, string text, params object[] args)
        {
            return Localization.Tr(map, text, args);
        }
        public static string[] TrM(LanguageMap map, string text, params string[] texts)
        {
            return Localization.TrM(map, text, texts);
        }
        public static string[] TrM(LanguageMap map, string[] texts)
        {
            return Localization.TrM(map, texts);
        }
    }

    [System.Serializable]
    public class LanguageData
    {
        [SerializeField]
        private string remark;
        public LanguageMap language;
        public List<string> selectors = new List<string>();
    }
}
