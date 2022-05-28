using System.Collections;
using UnityEngine;

namespace ZetanStudio.Item.Module
{
    public abstract class ItemModuleData
    {
        public ItemData Item { get; protected set; }

        public abstract ItemModule GetModule();

        public static implicit operator bool(ItemModuleData self)
        {
            return self != null;
        }
    }

    public abstract class ItemModuleData<T> : ItemModuleData where T : ItemModule
    {
        public T Module { get; protected set; }

        public sealed override ItemModule GetModule() => Module;

        protected ItemModuleData(ItemData item, T model)
        {
            Item = item;
            Module = model;
        }
    }
}