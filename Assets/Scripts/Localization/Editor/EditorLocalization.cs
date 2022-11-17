using UnityEngine;
using UnityEditor;

namespace ZetanStudio
{
    public sealed class EditorLocalization : SingletonScriptableObject<EditorLocalization>
    {
        [InitializeOnLoadMethod]
        private static void GetOrCreateInstance()
        {
            if (!Instance)
                AssetDatabase.CreateAsset(CreateInstance<EditorLocalization>(), AssetDatabase.GenerateUniqueAssetPath($"Assets/Scripts/Localization/Editor/Resources/New {ObjectNames.NicifyVariableName(typeof(EditorLocalization).Name)}.asset"));
        }

        [SerializeField]
        private LanguageMap language;

        public static string Tr(string text)
        {
            if (!Instance) return text;
            return L.Tr(Instance.language, text);
        }

        public static string Tr(string text, params object[] args)
        {
            if (!Instance) return string.Format(text, args);
            return L.Tr(Instance.language, text, args);
        }
    }

    public sealed class EDL
    {
        public static string Tr(string text)
        {
            return EditorLocalization.Tr(text);
        }

        public static string Tr(string text, params object[] args)
        {
            return EditorLocalization.Tr(text, args);
        }
    }
}
