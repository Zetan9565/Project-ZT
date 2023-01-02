using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.LootSystem;

namespace ZetanStudio.GatheringSystem
{
    [CreateAssetMenu(fileName = "resource info", menuName = "Zetan Studio/采集物信息")]
    public class ResourceInformation : ScriptableObject
    {
        [SerializeField]
        protected string _ID;
        public string ID
        {
            get
            {
                return _ID;
            }
        }

        [SerializeField]
        protected string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        [SerializeField]
        protected GatherType gatherType;
        public GatherType GatherType
        {
            get
            {
                return gatherType;
            }
        }

        [SerializeField]
        protected float gatherTime = 5.0f;
        public float GatherTime
        {
            get
            {
                return gatherTime;
            }
        }

        [SerializeField]
        protected float refreshTime;
        public float RefreshTime
        {
            get
            {
                return refreshTime;
            }
        }

        [SerializeField]
        protected List<DropItemInfo> productItems = new List<DropItemInfo>();
        public List<DropItemInfo> ProductItems
        {
            get
            {
                return productItems;
            }
        }

        [SerializeField]
        private LootAgent lootPrefab;
        public LootAgent LootPrefab
        {
            get
            {
                if (!lootPrefab) return MiscSettings.Instance.DefaultLootPrefab;
                return lootPrefab;
            }
        }

        public bool IsValid => !string.IsNullOrEmpty(_ID) && !string.IsNullOrEmpty(_name) && productItems.TrueForAll(x => x.IsValid) && lootPrefab;

#if UNITY_EDITOR
        public void SetBaseName(string name)
        {
            base.name = name;
        }
#endif
    }
    public enum GatherType
    {
        /// <summary>
        /// 手采
        /// </summary>
        [InspectorName("手摘")]
        Hands,
        /// <summary>
        /// 斧子
        /// </summary>
        [InspectorName("用斧子砍")]
        Axe,
        /// <summary>
        /// 镐子
        /// </summary>
        [InspectorName("用稿子敲")]
        Shovel,
        /// <summary>
        /// 铲子
        /// </summary>
        [InspectorName("用铲子挖")]
        Spade,
        /// <summary>
        /// 锄头
        /// </summary>
        [InspectorName("用锄头翻")]
        Hoe
    }
}