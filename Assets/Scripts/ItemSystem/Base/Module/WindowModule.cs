using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("打开窗口"), Require(typeof(UsableModule))]
    public class WindowModule : ItemModule
    {
        public override bool IsValid => !string.IsNullOrEmpty(WindowType);

        [field: SerializeField, TypeSelector(typeof(Window))]
        public string WindowType { get; private set; }
    }
}