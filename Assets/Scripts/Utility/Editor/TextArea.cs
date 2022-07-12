using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.Editor
{
    public sealed class TextArea : TextField
    {
        public new class UxmlFactory : UxmlFactory<TextArea, UxmlTraits> { }

#pragma warning disable IDE1006 // 命名样式
        public new bool multiline => base.multiline;
#pragma warning restore IDE1006 // 命名样式

        private TextInputBase textInput;

        public TextArea() => Init();
        public TextArea(string label) : base(label) => Init();
        public TextArea(string label, float defaultHeight) : this(label) => textInput.style.minHeight = defaultHeight;
        private void Init()
        {
            base.multiline = true;
            style.flexDirection = FlexDirection.Column;
            textInput = this.Q<TextInputBase>("unity-text-input");
            textInput.style.unityTextAlign = TextAnchor.UpperLeft;
            textInput.style.whiteSpace = WhiteSpace.Normal;
            labelElement.style.marginBottom = 2;
        }
    }
}