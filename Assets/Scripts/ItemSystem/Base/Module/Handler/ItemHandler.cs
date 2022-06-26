using UnityEngine;

namespace ZetanStudio.ItemSystem
{
    public abstract class ItemHandler : ScriptableObject
    {
        [SerializeField]
        protected string _name;
        public string Name => _name;

        protected virtual ItemHandler Instance => this;

        public bool Handle(ItemData item)
        {
            return Instance.DoHandle(item);
        }

        protected abstract bool DoHandle(ItemData item);
    }
}