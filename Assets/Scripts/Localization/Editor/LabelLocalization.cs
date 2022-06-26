namespace ZetanStudio
{
    public class LabelLocalization : SingletonScriptableObject<LabelLocalization>
    {
        public LanguageMap language;

        public static string Tr(string text)
        {
            if (!instance) return text;
            return L.Tr(instance.language, text);
        }
    }
}
