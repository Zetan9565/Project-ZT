using System.Collections;
using UnityEngine;

namespace ZetanStudio.Item.Module
{
    public abstract class ItemModuleData
    {
        public ItemData Item { get; protected set; }

        public abstract ItemModule GetModule();
    }

    public abstract class ItemModuleData<T> : ItemModuleData where T : ItemModule
    {
        public T Model { get; protected set; }

        public sealed override ItemModule GetModule() => Model;

        protected ItemModuleData(ItemData item, T model)
        {
            Item = item;
            Model = model;
        }
    }
}