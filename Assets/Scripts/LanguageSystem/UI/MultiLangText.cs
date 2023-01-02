using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio
{
    public class MultiLangText : Text
    {
        [SerializeField]
        private string m_selector;

#pragma warning disable IDE1006 // 命名样式
        public string selector
#pragma warning restore IDE1006 // 命名样式
        {
            get => m_selector;
            set
            {
                if (m_selector != value)
                    if (argsCache != null && argsCache.Length > 0) base.text = L.Tr(m_selector = value, cache, argsCache);
                    else base.text = L.Tr(m_selector = value, cache);
            }
        }

        public override string text { get => base.text; set => SetText(value); }

        private string cache;
        private object[] argsCache;

        protected override void Awake()
        {
            base.Awake();
            text = text;
            Language.OnLanguageChanged += () =>
            {
                if (argsCache != null && argsCache.Length > 0) base.text = L.Tr(m_selector, cache, argsCache);
                else base.text = L.Tr(m_selector, cache);
            };
        }

        public void SetText(string text)
        {
            base.text = L.Tr(m_selector, cache = text);
            argsCache = null;
        }

        public void SetText(string text, params object[] args) => base.text = L.Tr(m_selector, text, argsCache = args);
    }
}
