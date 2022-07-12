using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ZetanStudio
{
    public static class Keywords
    {
        private readonly static Dictionary<string, Dictionary<string, (string name, Color color)>> keywords = new Dictionary<string, Dictionary<string, (string, Color)>>();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            keywords.Clear();
            foreach (var method in ZetanUtility.GetMethodsWithAttribute<RuntimeGetKeywordsMethodAttribute>())
            {
                try
                {
                    foreach (var keywords in method.Invoke(null, null) as IEnumerable<IKeywords>)
                    {
                        if (Keywords.keywords.TryGetValue(keywords.IDPrefix, out var dict))
                            dict[keywords.ID] = (keywords.Name, keywords.Color);
                        else Keywords.keywords[keywords.IDPrefix] = new Dictionary<string, (string, Color)>() { { keywords.ID, (keywords.Name, keywords.Color) } };
                    }
                }
                catch { }
            }
        }

        public static string Translate(string keywords, bool color = false)
        {
            var match = Regex.Match(keywords, @"^{\[\w+\]\w+}$");
            if (match.Success)
            {
                var temp = keywords[2..^1];
                string[] split = temp.Split(']');
                if (Keywords.keywords.TryGetValue(split[0], out var dict) && dict.TryGetValue(split[1], out var result))
                    return ZetanUtility.ColorText(result.name, color ? result.color : default);
            }
            return keywords;
        }


        public static string Generate(IKeywords keywords)
        {
            return $"{{[{keywords.IDPrefix}]{keywords.ID}}}";
        }

        public static string HandleKeyWords(string input, bool color = false)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            StringBuilder output = new StringBuilder();
            StringBuilder keyWordsGetter = new StringBuilder();
            bool startGetting = false;
            for (int i = 0; i < input.Length; i++)
            {
                if (i + 1 < input.Length && input[i] == '{' && input[i + 1] != '{') startGetting = true;
                else if (startGetting && input[i] == '}' && (i + 1 >= input.Length || input[i + 1] != '}'))
                {
                    startGetting = false;
                    keyWordsGetter.Append(input[i]);
                    output.Append(Translate(keyWordsGetter.ToString(), color));
                    keyWordsGetter.Clear();
                }
                else if (!startGetting) output.Append(input[i]);
                if (startGetting) keyWordsGetter.Append(input[i]);
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
        private readonly static Dictionary<string, Dictionary<string, IKeywords>> keywords_e = new Dictionary<string, Dictionary<string, IKeywords>>();

        [UnityEditor.InitializeOnLoad]
        public static class Editor
        {
            static Editor()
            {
                UnityEditor.EditorApplication.projectChanged += Init;
            }

            [UnityEditor.InitializeOnLoadMethod]
            public static void Init()
            {
                keywords_e.Clear();
                foreach (var method in UnityEditor.TypeCache.GetMethodsWithAttribute<GetKeywordsMethodAttribute>())
                {
                    try
                    {
                        foreach (var keywords in method.Invoke(null, null) as IEnumerable<IKeywords>)
                        {
                            if (keywords_e.TryGetValue(keywords.IDPrefix, out var dict)) dict[keywords.ID] = keywords;
                            else keywords_e[keywords.IDPrefix] = new Dictionary<string, IKeywords>() { { keywords.ID, keywords } };
                        }
                    }
                    catch { }
                }
            }

            public static string Translate(string keywords)
            {
                var match = Regex.Match(keywords, @"^{\[\w+\]\w+}$");
                if (match.Success)
                {
                    var temp = keywords[2..^1];
                    string[] split = temp.Split(']');
                    if (keywords_e.TryGetValue(split[0], out var dict) && dict.TryGetValue(split[1], out var result))
                        return result.Name;
                }
                return keywords;
            }

            public static string HandleKeyWords(string input)
            {
                if (string.IsNullOrEmpty(input)) return string.Empty;
                StringBuilder output = new StringBuilder();
                StringBuilder keyWordsGetter = new StringBuilder();
                bool startGetting = false;
                for (int i = 0; i < input.Length; i++)
                {
                    if (i + 1 < input.Length && input[i] == '{' && input[i + 1] != '{') startGetting = true;
                    else if (startGetting && input[i] == '}' && (i + 1 >= input.Length || input[i + 1] != '}'))
                    {
                        startGetting = false;
                        keyWordsGetter.Append(input[i]);
                        output.Append(Translate(keyWordsGetter.ToString()));
                        keyWordsGetter.Clear();
                    }
                    else if (!startGetting) output.Append(input[i]);
                    if (startGetting) keyWordsGetter.Append(input[i]);
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
    public interface IKeywords
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