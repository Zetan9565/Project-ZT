using UnityEngine;

namespace ZetanStudio.Item
{
    public abstract class ItemHandler : ScriptableObject
    {
        protected bool IsInstance { get; private set; }

        public ItemHandler GetInstance()
        {
            var instance = Instantiate(this);
            instance.IsInstance = true;
            return instance;
        }

        public bool Handle(ItemData item)
        {
            return IsInstance && DoHandle(item);
        }

        protected abstract bool DoHandle(ItemData item);
    }
}