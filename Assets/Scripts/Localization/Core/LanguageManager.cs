using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private Localization localization;

        private Dictionary<string, List<Dictionary<string, string>>> existDicts = new Dictionary<string, List<Dictionary<string, string>>>();

        private void Awake()
        {
            Init(localization);
        }

        public void Init(Localization localization = null)
        {
            this.localization = localization;
            if (this.localization) existDicts = this.localization.AsDictionary();
            else existDicts = new Dictionary<string, List<Dictionary<string, string>>>();
        }
        private List<Dictionary<string, string>> FindDictionaries(string selector)
        {
            if (!existDicts.TryGetValue(selector, out var dict))
            {
                if (localization)
                {
                    var dicts = localization.FindDictionaries(selector);
                    if (dicts.Count > 0) existDicts[selector] = dict = dicts;
                }
            }
            return dict;
        }

        public string Tr(string selector, string text)
        {
            return Tr(text, FindDictionaries(selector));
        }
        public string Tr(string selector, string text, params object[] args)
        {
            return string.Format(Tr(selector, text), args);
        }
        public IEnumerable<string> TrM(string selector, string text, params string[] texts)
        {
            var dicts = FindDictionaries(selector);
            List<string> results = new List<string>();
            if (dicts == null)
            {
                results.Add(text);
                results.AddRange(texts);
            }
            else
            {
                results.Add(Tr(text, dicts));
                foreach (var t in texts)
                {
                    results.Add(Tr(t, dicts));
                }
            }
            return results.AsEnumerable();
        }

        private static string Tr(string text, List<Dictionary<string, string>> dicts)
        {
            if (dicts != null)
                foreach (var dict in dicts)
                {
                    if (dict.TryGetValue(text, out var content))
                        return content;
                    var match = Regex.Match(text, @"(?<=^<color=[\w]*>)(.*)(?=</color>$)");
                    if (match.Success && dict.TryGetValue(match.Value, out content))
                        return text.Replace(match.Value, content); ;
                }
            return text;
        }
    }
}
