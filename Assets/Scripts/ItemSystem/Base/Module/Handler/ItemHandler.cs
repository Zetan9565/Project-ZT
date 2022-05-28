using UnityEngine;

namespace ZetanStudio.Item
{
    public abstract class ItemHandler : ScriptableObject
    {
        public abstract string Name { get; }

        protected virtual ItemHandler Instance => this;

        public bool Handle(ItemData item)
        {
            return Instance.DoHandle(item);
        }

        protected abstract bool DoHandle(ItemData item);
    }
}