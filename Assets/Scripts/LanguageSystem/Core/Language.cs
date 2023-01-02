using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ZetanStudio
{
    public static class L
    {
        public static string Tr(string selector, string text) => Language.Tr(selector, text);
        public static string Tr(string selector, string text, params object[] args) => Language.Tr(selector, text, args);
        public static string[] TrM(string selector, string text, params string[] texts) => Language.TrM(selector, text, texts);
        public static string[] TrM(string selector, string[] texts) => Language.TrM(selector, texts);

        #region 供Editor使用
        public static string Tr(LanguageSet set, string text) => Language.Tr(set, 0, text);
        public static string Tr(LanguageSet set, string text, params object[] args) => Language.Tr(set, 0, text, args);
        public static string[] TrM(LanguageSet set, string text, params string[] texts) => Language.TrM(set, 0, text, texts);
        public static string[] TrM(LanguageSet set, string[] texts) => Language.TrM(set, 0, texts);
        public static string Tr(LanguageSet set, int lang, string text) => Language.Tr(set, lang, text);
        public static string Tr(LanguageSet set, int lang, string text, params object[] args) => Language.Tr(set, lang, text, args);
        public static string[] TrM(LanguageSet set, int lang, string text, params string[] texts) => Language.TrM(set, lang, text, texts);
        public static string[] TrM(LanguageSet set, int lang, string[] texts) => Language.TrM(set, lang, texts);
        #endregion
    }

    public static class Language
    {
        private static Dictionary<string, List<Dictionary<string, string>>> dictionaries = new Dictionary<string, List<Dictionary<string, string>>>();
        private static int langIndex = -1;
        public static int LangIndex
        {
            get => langIndex;
            set
            {
                if (langIndex != value)
                {
                    langIndex = value;
                    Init();
                    OnLanguageChanged?.Invoke();
                }
            }
        }

        public static event Action OnLanguageChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void Init()
        {
            if (Localization.Instance) dictionaries = Localization.Instance.AsDictionary(LangIndex);
            else dictionaries = new Dictionary<string, List<Dictionary<string, string>>>();
        }

        private static List<Dictionary<string, string>> FindDictionaries(string selector)
        {
            if (string.IsNullOrEmpty(selector)) return null;
            if (!dictionaries.TryGetValue(selector, out var list))
            {
                if (Localization.Instance)
                {
                    var dicts = Localization.Instance.FindDictionaries(LangIndex, selector);
                    if (dicts.Count > 0) dictionaries[selector] = list = dicts;
                }
            }
            return list;
        }

        private static string Tr(string text, List<Dictionary<string, string>> dicts)
        {
            if (dicts != null)
            {
                foreach (var dict in dicts)
                {
                    if (dict.TryGetValue(text, out var result)) return result;
                    var match = Regex.Match(text, @"(?<=^<color=[\w]*>)(.*)(?=</color>$)");
                    if (match.Success && dict.TryGetValue(match.Value, out result))
                        return text.Replace(match.Value, result); ;
                }
            }
            return text;
        }

        public static string Tr(string selector, string text) => Tr(text, FindDictionaries(selector));
        public static string Tr(string selector, string text, params object[] args) => string.Format(Tr(selector, text), args);

        public static string[] TrM(string selector, string text, params string[] texts)
        {
            var dicts = FindDictionaries(selector);
            List<string> result = new List<string>();
            if (dicts == null)
            {
                result.Add(text);
                result.AddRange(texts);
            }
            else
            {
                result.Add(Tr(text, dicts));
                result.AddRange(texts.Select(t => Tr(t, dicts)));
            }
            return result.ToArray();
        }
        public static string[] TrM(string selector, string[] texts)
        {
            var dicts = FindDictionaries(selector);
            if (dicts == null) return texts;
            else
            {
                List<string> result = new List<string>();
                result.AddRange(texts.Select(t => Tr(t, dicts)));
                return result.ToArray();
            }
        }

        #region 供Editor使用
        public static string Tr(LanguageSet set, string text) => Tr(set, 0, text);
        public static string Tr(LanguageSet set, string text, params object[] args) => Tr(set, 0, text, args);
        public static string[] TrM(LanguageSet set, string text, params string[] texts) => TrM(set, 0, text, texts);
        public static string[] TrM(LanguageSet set, string[] texts) => TrM(set, 0, texts);
        public static string Tr(LanguageSet set, int lang, string text) => set ? set.Tr(lang, text) : text;
        public static string Tr(LanguageSet set, int lang, string text, params object[] args) => string.Format(Tr(set, lang, text), args);
        public static string[] TrM(LanguageSet set, int lang, string text, params string[] texts)
        {
            List<string> result = new List<string>();
            if (!set)
            {
                result.Add(text);
                result.AddRange(texts);
            }
            else
            {
                result.Add(set.Tr(lang, text));
                result.AddRange(texts.Select(t => set.Tr(lang, t)));
            }
            return result.ToArray();
        }
        public static string[] TrM(LanguageSet set, int lang, string[] texts)
        {
            List<string> result = new List<string>();
            if (!set) return texts;
            else result.AddRange(texts.Select(t => set.Tr(lang, t)));
            return result.ToArray();
        }
        #endregion
    }
}