using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using ZetanStudio.PlayerSystem;

namespace ZetanStudio
{
    public static class Keyword
    {
        private readonly static Dictionary<string, Dictionary<string, (string name, Color color)>> keywords = new Dictionary<string, Dictionary<string, (string, Color)>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            keywords.Clear();
            foreach (var method in Utility.GetMethodsWithAttribute<RuntimeGetKeywordsMethodAttribute>())
            {
                try
                {
                    foreach (var keyword in method.Invoke(null, null) as IEnumerable<IKeyword>)
                    {
                        if (keywords.TryGetValue(keyword.IDPrefix, out var dict))
                            dict[keyword.ID] = (keyword.Name, keyword.Color);
                        else keywords[keyword.IDPrefix] = new Dictionary<string, (string, Color)>() { { keyword.ID, (keyword.Name, keyword.Color) } };
                    }
                }
                catch { }
            }
        }

        public static string Translate(string keyword, bool color = false)
        {
            if (keyword.ToUpper() == "{[PLAYER]}") return PlayerManager.Instance.PlayerInfo.Name;
            var match = Regex.Match(keyword, @"^{\[\w+\]\w+}$");
            if (match.Success)
            {
                var temp = keyword[2..^1];
                string[] split = temp.Split(']');
                if (keywords.TryGetValue(split[0], out var dict) && dict.TryGetValue(split[1], out var result))
                    return Utility.ColorText(result.name, color ? result.color : default);
            }
            return keyword;
        }

        public static string Generate(IKeyword keywords)
        {
            return $"{{[{keywords.IDPrefix}]{keywords.ID}}}";
        }

        public static string HandleKeywords(string input, bool color = false)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            StringBuilder output = new StringBuilder();
            StringBuilder keywordGetter = new StringBuilder();
            bool startGetting = false;
            for (int i = 0; i < input.Length; i++)
            {
                if (i + 1 < input.Length && input[i] == '{' && input[i + 1] != '{') startGetting = true;
                else if (startGetting && input[i] == '}' && (i + 1 >= input.Length || input[i + 1] != '}'))
                {
                    startGetting = false;
                    keywordGetter.Append(input[i]);
                    output.Append(Translate(keywordGetter.ToString(), color));
                    keywordGetter.Clear();
                }
                else if (!startGetting) output.Append(input[i]);
                if (startGetting) keywordGetter.Append(input[i]);
            }

            return output.ToString();
        }

        public static IEnumerable<KeyValuePair<string, string>> ExtractKeyWords(string input)
        {
            if (string.IsNullOrEmpty(input)) return new KeyValuePair<string, string>[0];
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            StringBuilder keyWordsGetter = new StringBuilder();
            bool startGetting = false;
            for (int i = 0; i < input.Length; i++)
            {
                if (i + 1 < input.Length && input[i] == '{' && input[i + 1] != '{') startGetting = true;
                else if (startGetting && input[i] == '}' && (i + 1 >= input.Length || input[i + 1] != '}'))
                {
                    startGetting = false;
                    keyWordsGetter.Append(input[i]);
                    pairs.Add(KeyValuePair.Create(keyWordsGetter.ToString(), Translate(keyWordsGetter.ToString())));
                    keyWordsGetter.Clear();
                }
                if (startGetting) keyWordsGetter.Append(input[i]);
            }
            return pairs;
        }

#if UNITY_EDITOR

        [UnityEditor.InitializeOnLoad]
        public static class Editor
        {
            private readonly static Dictionary<string, Dictionary<string, IKeyword>> keywords = new Dictionary<string, Dictionary<string, IKeyword>>();

            static Editor()
            {
                UnityEditor.EditorApplication.projectChanged += Init;
            }

            [UnityEditor.InitializeOnLoadMethod]
            public static void Init()
            {
                keywords.Clear();
                foreach (var method in UnityEditor.TypeCache.GetMethodsWithAttribute<GetKeywordsMethodAttribute>())
                {
                    try
                    {
                        foreach (var keyword in method.Invoke(null, null) as IEnumerable<IKeyword>)
                        {
                            if (keywords.TryGetValue(keyword.IDPrefix, out var dict)) dict[keyword.ID] = keyword;
                            else keywords[keyword.IDPrefix] = new Dictionary<string, IKeyword>() { { keyword.ID, keyword } };
                        }
                    }
                    catch { }
                }
            }

            public static string Translate(string keyword)
            {
                var match = Regex.Match(keyword, @"^{\[\w+\]\w+}$");
                if (match.Success)
                {
                    var temp = keyword[2..^1];
                    string[] split = temp.Split(']');
                    if (keywords.TryGetValue(split[0], out var dict) && dict.TryGetValue(split[1], out var result))
                        return result.Name;
                }
                return keyword;
            }

            public static string HandleKeywords(string input)
            {
                if (string.IsNullOrEmpty(input)) return string.Empty;
                StringBuilder output = new StringBuilder();
                StringBuilder keywordGetter = new StringBuilder();
                bool startGetting = false;
                for (int i = 0; i < input.Length; i++)
                {
                    if (i + 1 < input.Length && input[i] == '{' && input[i + 1] != '{') startGetting = true;
                    else if (startGetting && input[i] == '}' && (i + 1 >= input.Length || input[i + 1] != '}'))
                    {
                        startGetting = false;
                        keywordGetter.Append(input[i]);
                        output.Append(Translate(keywordGetter.ToString()));
                        keywordGetter.Clear();
                    }
                    else if (!startGetting) output.Append(input[i]);
                    if (startGetting) keywordGetter.Append(input[i]);
                }

                return output.ToString();
            }

            public static IEnumerable<KeyValuePair<string, string>> ExtractKeyWords(string input)
            {
                if (string.IsNullOrEmpty(input)) return new KeyValuePair<string, string>[0];
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                StringBuilder keyWordsGetter = new StringBuilder();
                bool startGetting = false;
                for (int i = 0; i < input.Length; i++)
                {
                    if (i + 1 < input.Length && input[i] == '{' && input[i + 1] != '{') startGetting = true;
                    else if (startGetting && input[i] == '}' && (i + 1 >= input.Length || input[i + 1] != '}'))
                    {
                        startGetting = false;
                        keyWordsGetter.Append(input[i]);
                        pairs.Add(KeyValuePair.Create(keyWordsGetter.ToString(), Translate(keyWordsGetter.ToString())));
                        keyWordsGetter.Clear();
                    }
                    if (startGetting) keyWordsGetter.Append(input[i]);
                }
                return pairs;
            }
        }
#endif
    }
    public interface IKeyword
    {
        public string ID { get; }

        public string IDPrefix { get; }

        public string Name { get; }

        public Color Color { get; }

        public string Group { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class KeywordsGroupAttribute : Attribute
    {
        public readonly string group;

        public KeywordsGroupAttribute(string group)
        {
            this.group = group;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class GetKeywordsMethodAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RuntimeGetKeywordsMethodAttribute : Attribute { }
}