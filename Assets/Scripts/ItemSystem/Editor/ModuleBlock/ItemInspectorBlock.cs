using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.ItemSystem.Editor
{
    public abstract class ItemInspectorBlock : Foldout
    {
        protected ItemEditorSettings settings;

        public ItemInspectorBlock()
        {
            this.Q<Toggle>().style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
            this.Q<Toggle>().style.backgroundColor = new StyleColor(new Color(0.75f, 0.75f, 0.75f, 0.05f));
            style.borderBottomWidth = 1;
            style.borderBottomColor = Color.black;
            contentContainer.style.paddingRight = 2;
            contentContainer.style.paddingBottom = 5;
            settings = ItemEditorSettings.GetOrCreate();
        }

        protected string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }
        protected string Tr(string text, params object[] args)
        {
            return L.Tr(settings.language, text, args);
        }
    }
}