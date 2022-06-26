using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    using Extension;

    [Name("强化"), Require(typeof(AttributeModule))]
    public class EnhancementModule : ItemModule
    {
        public override bool IsValid => (enhancementInfo && enhancementInfo.IsValid || !enhancementInfo && (enhancement?.IsValid ?? false)) &&
            (Method == EnhanceMethod.SingleItem && costs.Length > 0 && costs.None(x => !x.IsValid) ||
            Method == EnhanceMethod.Materials && materials.Length > 0 && materials.None(x => !x.IsValid) ||
            Method == EnhanceMethod.Experience && expTypes.Length > 0 && experienceTable.Length > 0 && experienceTable.None(x => !x.IsValid));

        public int UpperLimit => Mathf.Min(enhancementInfo ? enhancementInfo.Enhancement.Increments.Count : (enhancement?.Increments.Count ?? 0),
            Method == EnhanceMethod.SingleItem ? costs.Length : Method == EnhanceMethod.Materials ? materials.Length : experienceTable.Length);

        [SerializeField]
        private ItemEnhancementInfo enhancementInfo;

        [SerializeField]
        private ItemEnhancement enhancement;

        public IEnumerable<ItemProperty> GenerateIncrements(int level)
        {
            List<ItemProperty> results = new List<ItemProperty>();
            if (!IsValid) return results;
            IEnumerable<ItemAttribute> increments;
            if (enhancementInfo) increments = enhancementInfo.Enhancement.Increments[level - 1].Increments;
            else increments = (IEnumerable<ItemAttribute>)enhancement?.Increments[level - 1].Increments ?? new ItemAttribute[0];
            foreach (var incre in increments)
            {
                results.Add(new ItemProperty(incre.Type) { Value = incre.Value });
            }
            return results;
        }

        [field: SerializeField]
        public EnhanceMethod Method { get; private set; }

        [field: SerializeField]
        public EnhanceFailure Failure { get; private set; }

        [SerializeField]
        private EnhLevelConsumables[] costs;
        public ReadOnlyCollection<EnhLevelConsumables> Costs => new ReadOnlyCollection<EnhLevelConsumables>(costs);

        [SerializeField]
        private EnhLevelMaterials[] materials;
        public ReadOnlyCollection<EnhLevelMaterials> Materials => new ReadOnlyCollection<EnhLevelMaterials>(materials);

        [SerializeField]
        private int[] expTypes;
        public ReadOnlyCollection<int> ExpTypes => new ReadOnlyCollection<int>(expTypes);

        [SerializeField]
        private EnhLevelExperience[] experienceTable = { };
        public ReadOnlyCollection<EnhLevelExperience> ExperienceTable => new ReadOnlyCollection<EnhLevelExperience>(experienceTable);

        public override ItemModuleData CreateData(ItemData item)
        {
            return new EnhancementData(item, this);
        }

        public static string GetEnhancedName(ItemData item)
        {
            return (item.TryGetModuleData<EnhancementData>(out var data) ? $"+{data.level} " : string.Empty) + item.Name;
        }
        public static bool IsEnhanceable(ItemData item)
        {
            return item.TryGetModuleData<EnhancementData>(out var data) && !data.IsMax;
        }
    }

    public class EnhancementData : ItemModuleData<EnhancementModule>
    {
        public int level;

        public bool IsMax => level >= Module.UpperLimit;

        public EnhancementData(ItemData item, EnhancementModule module) : base(item, module)
        {
        }

        public override SaveDataItem GetSaveData()
        {
            var data = new SaveDataItem();
            data.intData["level"] = level;
            return data;
        }
        public override void LoadSaveData(SaveDataItem data)
        {
            level = data.intData["level"];
        }
    }

    public enum EnhanceMethod
    {
        SingleItem,
        Materials,
        Experience
    }

    public enum EnhanceFailure
    {
        [InspectorName("无")]
        None,
        [InspectorName("降级")]
        Downgrade,
        [InspectorName("破损")]
        Broken,
        [InspectorName("消失")]
        Dsiappear
    }

    [Serializable]
    public class EnhLevelMaterials
    {
        [SerializeField]
        private EnhMaterialSet[] materials;
        public ReadOnlyCollection<EnhMaterialSet> Materials => new ReadOnlyCollection<EnhMaterialSet>(materials);

        public bool IsValid => materials.None(x => !x.IsValid);
    }

    [Serializable]
    public class EnhMaterialSet
    {
        public bool IsValid => SuccessRate > 0f && materials.Length > 0 && materials.None(x => !x.IsValid);

        [field: SerializeField, Range(0.0001f, 1)]
        public float SuccessRate { get; private set; } = 1f;

        [SerializeField]
        private MaterialInfo[] materials;
        public ReadOnlyCollection<MaterialInfo> Materials => new ReadOnlyCollection<MaterialInfo>(materials);
    }

    [Serializable]
    public class EnhLevelConsumables
    {
        public bool IsValid => materials.Length > 0 && materials.None(x => !x.IsValid);

        [SerializeField]
        private EnhConsumable[] materials;
        public ReadOnlyCollection<EnhConsumable> Materials => new ReadOnlyCollection<EnhConsumable>(materials);
    }

    [Serializable]
    public class EnhConsumable : IItemInfo
    {
        [field: SerializeField]
        public float SuccessRate { get; private set; } = 1f;

        public string ItemID => Item.ID;

        public string ItemName => Item.Name;

        [field: SerializeField, ItemFilter(typeof(EnhConsumableModule))]
        public Item Item { get; private set; }

        [field: SerializeField, Min(1)]
        public int Amount { get; private set; }

        public bool IsValid => Item && Amount > 0 && SuccessRate > 0f;
    }

    [Serializable]
    public class EnhLevelExperience
    {
        [field: SerializeField, Min(1)]
        public int Experience { get; private set; }

        [field: SerializeField, Range(0.0001f, 1)]
        public float SuccessRate { get; private set; } = 1f;

        public bool IsValid => Experience > 0 && SuccessRate > 0;
    }
}