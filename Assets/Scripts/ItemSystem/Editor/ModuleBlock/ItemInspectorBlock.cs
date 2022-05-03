using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.Item
{
    public abstract class ItemInspectorBlock : Foldout
    {
        public ItemInspectorBlock()
        {
            this.Q<Toggle>().style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
            this.Q<Toggle>().style.backgroundColor = new StyleColor(new Color(0.75f, 0.75f, 0.75f, 0.05f));
            style.borderBottomWidth = 1;
            style.borderBottomColor = Color.black;
            contentContainer.style.paddingRight = 2;
            contentContainer.style.paddingBottom = 5;
        }
    }
}