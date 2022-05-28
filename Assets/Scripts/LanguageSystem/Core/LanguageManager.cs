using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZetanStudio
{
    public static class LM
    {
        public static string Tr(string selector, string text)
        {
            if (LanguageManager.Instance) return LanguageManager.Instance.Tr(selector, text);
            return text;
        }
        public static string Tr(string selector, string text, params object[] args)
        {
            if (LanguageManager.Instance) return LanguageManager.Instance.Tr(selector, text, args);
            return string.Format(text, args);
        }
        public static IEnumerable<string> TrM(string selector, string text, params string[] texts)
        {
            if (LanguageManager.Instance) return LanguageManager.Instance.TrM(selector, text, texts);
            List<string> results = new List<string>() { text };
            results.AddRange(texts);
            return results.AsEnumerable();
        }
    }

    public sealed class LanguageManager : SingletonMonoBehaviour<LanguageManager>
    {
        [SerializeField]
        private Language language;

        private Dictionary<LanguageMap, Dictionary<string, string>> languageDicts = new Dictionary<LanguageMap, Dictionary<string, string>>();

        private Dictionary<string, Dictionary<string, string>> existDicts = new Dictionary<string, Dictionary<string, string>>();

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (language)
            {
                languageDicts = language.AsDictionary();
                existDicts.Clear();
                foreach (var data in language.Datas)
                {
                    foreach (var selector in data.selectors)
                    {
                        existDicts[selector] = languageDicts[data.language];
                    }
                }
            }
        }
        private Dictionary<string, string> FindDict(string selector)
        {
            if (!existDicts.TryGetValue(selector, out var dict))
            {
                if (language)
                {
                    var map = language.FindMap(selector);
                    if (map)
                    {
                        if (!languageDicts.TryGetValue(map, out var mdict))
                        {
                            mdict = map.AsDictionary();
                            languageDicts[map] = mdict;
                        }
                        dict = mdict;
                        existDicts[selector] = dict;
                    }
                }
            }

            return dict;
        }

        public string Tr(string selector, string text)
        {
            Dictionary<string, string> dict = FindDict(selector);
            if (dict != null && dict.TryGetValue(text, out var content)) return content;
            else return text;
        }
        public string Tr(string selector, string text, params object[] args)
        {
            return string.Format(Tr(selector, text), args);
        }
        public IEnumerable<string> TrM(string selector, string text, params string[] texts)
        {
            Dictionary<string, string> dict = FindDict(selector);
            List<string> results = new List<string>();
            if (dict == null)
            {
                results.Add(text);
                results.AddRange(texts);
            }
            else
            {
                results.Add(tr(text));
                foreach (var t in texts)
                {
                    results.Add(tr(t));
                }
                string tr(string text)
                {
                    if (dict.TryGetValue(text, out var content)) return content;
                    return text;
                }
            }
            return results.AsEnumerable();
        }
    }
}
