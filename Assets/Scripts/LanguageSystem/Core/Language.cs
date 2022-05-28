using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ZetanStudio
{
    [CreateAssetMenu]
    public sealed class Language : ScriptableObject
    {
        [SerializeField]
        private List<LanguageData> datas = new List<LanguageData>();
        public ReadOnlyCollection<LanguageData> Datas => datas.AsReadOnly();

        public LanguageMap FindMap(string selector)
        {
            return datas.Find(x => x.selectors.Contains(selector))?.language;
        }

        public Dictionary<LanguageMap, Dictionary<string, string>> AsDictionary()
        {
            return datas.ToDictionary(x => x.language, x => x.language.AsDictionary());
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
        public static IEnumerable<string> TrM(LanguageMap map, string text, params string[] texts)
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
            return results.AsEnumerable();
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
