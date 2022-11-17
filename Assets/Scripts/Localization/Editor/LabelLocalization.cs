namespace ZetanStudio
{
    public class LabelLocalization : SingletonScriptableObject<LabelLocalization>
    {
        public LanguageMap language;

        public static string Tr(string text)
        {
            if (!Instance) return text;
            return L.Tr(Instance.language, text);
        }
    }
}
