using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "opwn window", menuName = "Zetan Studio/道具/用途/打开窗口")]
    public class OpenWindow : ItemUsage
    {
        public OpenWindow()
        {
            _name = "打开窗口";
        }

        protected override bool Use(ItemData item)
        {
            if (!item.TryGetModule<WindowModule>(out var window)) return false;
            else return WindowsManager.OpenWindow(Utility.GetTypeByFullName(window.WindowType));
        }

        protected override bool Prepare(ItemData item, int cost)
        {
            return true;
        }
        protected override bool Complete(ItemData item, int cost)
        {
            return true;
        }
        protected override void Nodify(ItemData item) { }
    }
}