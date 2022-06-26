using UnityEngine;
using System.Collections.ObjectModel;

namespace ZetanStudio.ItemSystem.Module
{
    using Extension;

    [CreateAssetMenu(fileName = "item increment info", menuName = "Zetan Studio/道具/强化信息")]
    public class ItemEnhancementInfo : ScriptableObject
    {
        [field: SerializeField]
        public ItemEnhancement Enhancement { get; private set; }

        public bool IsValid => Enhancement?.IsValid ?? false;
    }

    [System.Serializable]
    public class ItemEnhancement
    {
        [SerializeField]
        private ItemIncrement[] increments;
        public ReadOnlyCollection<ItemIncrement> Increments => new ReadOnlyCollection<ItemIncrement>(increments);

        public bool IsValid => increments.Length > 0 && increments.None(x => x.Increments.Count < 1);
    }

    [System.Serializable]
    public class ItemIncrement
    {
        [SerializeField]
        private ItemAttribute[] attributes;
        public ReadOnlyCollection<ItemAttribute> Increments => new ReadOnlyCollection<ItemAttribute>(attributes);
    }
}