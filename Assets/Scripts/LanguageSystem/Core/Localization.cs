using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ZetanStudio
{
    public class Localization : SingletonScriptableObject<Localization>
    {
        [SerializeField]
        private LocalizationData[] datas = { };
        public ReadOnlyCollection<LocalizationData> Datas => new ReadOnlyCollection<LocalizationData>(datas);

        public Dictionary<string, List<Dictionary<string, string>>> AsDictionary(int lang)
        {
            Dictionary<LanguageSet, Dictionary<string, string>> dicts = new Dictionary<LanguageSet, Dictionary<string, string>>();
            Dictionary<string, List<Dictionary<string, string>>> result = new Dictionary<string, List<Dictionary<string, string>>>();
            foreach (var data in datas)
            {
                foreach (var selector in data.Selectors)
                {
                    if (!result.TryGetValue(selector, out var find))
                        result[selector] = new List<Dictionary<string, string>>() { makeDict(data.Language) };
                    else find.Add(makeDict(data.Language));
                }
            }
            return result;

            Dictionary<string, string> makeDict(LanguageSet set)
            {
                if (!dicts.TryGetValue(set, out var find))
                    dicts[set] = find = set.AsDictionary(lang);
                return find;
            }
        }

        public List<Dictionary<string, string>> FindDictionaries(int languageIndex, string selector)
        {
            return datas.Where(d => d.Selectors.Contains(selector)).Select(d => d.Language.AsDictionary(languageIndex)).ToList();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Zetan Studio/本地化")]
        private static void Create()
        {
            CreateSingleton();
        }
#endif
    }

    [Serializable]
    public class LocalizationData
    {
        [SerializeField]
        private string remark;

        [field: SerializeField]
        public LanguageSet Language { get; private set; }

        [SerializeField]
        private string[] selectors = { };
        public ReadOnlyCollection<string> Selectors => new ReadOnlyCollection<string>(selectors);
    }
}