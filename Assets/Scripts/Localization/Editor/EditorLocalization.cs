using UnityEngine;
using UnityEditor;

namespace ZetanStudio
{
    public sealed class EditorLocalization : SingletonScriptableObject<EditorLocalization>
    {
        [InitializeOnLoadMethod]
        private static void GetOrCreateInstance()
        {
            if (!instance) instance = ZetanUtility.Editor.LoadAsset<EditorLocalization>();
            if (!instance)
            {
                instance = CreateInstance<EditorLocalization>();
                AssetDatabase.CreateAsset(instance, AssetDatabase.GenerateUniqueAssetPath($"Assets/Scripts/Localization/Editor/Resources/New {ObjectNames.NicifyVariableName(typeof(EditorLocalization).Name)}.asset"));
            }
        }

        [SerializeField]
        private LanguageMap language;

        public static string Tr(string text)
        {
            if (!instance) return text;
            return L.Tr(instance.language, text);
        }

        public static string Tr(string text, params object[] args)
        {
            if (!instance) return string.Format(text, args);
            return L.Tr(instance.language, text, args);
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
