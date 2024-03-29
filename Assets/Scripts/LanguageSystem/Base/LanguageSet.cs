﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ZetanStudio
{
    [CreateAssetMenu]
    public sealed class LanguageSet : ScriptableObject
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        private LanguageMap[] maps = { };
        public ReadOnlyCollection<LanguageMap> Maps => new ReadOnlyCollection<LanguageMap>(maps);

        public Dictionary<string, string> AsDictionary(int lang)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var map in maps)
            {
                try
                {
                    result[map.Key] = map.Values[lang];
                }
                catch { }
            }
            return result;
        }

        public string Tr(string text) => Tr(0, text);
        public string Tr(int lang, string text)
        {
            var map = Array.Find(maps, m => m.Key == text);
            try
            {
                return map.Values[lang];
            }
            catch
            {
                return text;
            }
        }

#if UNITY_EDITOR
        public static class Editor
        {
            public static void SetName(LanguageSet language, string name)
            {
                if (language) language._name = name;
            }
            public static void SetMaps(LanguageSet language, LanguageMap[] maps)
            {
                if (language) language.maps = maps;
            }
        }
#endif
    }

    [Serializable]
    public sealed class LanguageMap
    {
        [field: SerializeField, TextArea]
        public string Key { get; private set; }

        [SerializeField, TextArea]
        private string[] values;
        public ReadOnlyCollection<string> Values => new ReadOnlyCollection<string>(values);

        public LanguageMap()
        {

        }

        public LanguageMap(string key, params string[] values)
        {
            Key = key;
            this.values = values;
        }
    }
}