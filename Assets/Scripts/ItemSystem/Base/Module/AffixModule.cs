using System;
using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.CharacterSystem;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("词缀"), Require(typeof(EquipableModule))]
    public class AffixModule : ItemModule
    {
        public override bool IsValid => affixInfo && affixInfo.IsValid || !affixInfo && (affix?.IsValid ?? false);

        [SerializeField]
        private ItemAffixInformation affixInfo;

        [SerializeField]
        private ItemAffix affix;

        public int UpperLimit => affixInfo ? affixInfo.UpperLimit : (affix?.UpperLimit ?? 0);

        public IEnumerable<ItemProperty> GenerateAffixes()
        {
            if (affixInfo) return affixInfo.GenerateAffixes();
            else return affix?.GenerateAffixes() ?? new List<ItemProperty>();
        }

        public override ItemModuleData CreateData(ItemData item)
        {
            return new AffixData(item, this);
        }
    }

    public class AffixData : ItemModuleData<AffixModule>
    {
        public readonly List<ItemProperty> affixes;

        public AffixData(ItemData item, AffixModule module) : base(item, module)
        {
            affixes = new List<ItemProperty>(module.GenerateAffixes());
        }

        public override GenericData GenerateSaveData()
        {
            var data = new GenericData();
            var ad = new GenericData();
            data["affixes"] = ad;
            foreach (var prop in affixes)
            {
                var pd = (affixes as IRoleValue).GenerateSaveData();
                ad[prop.ID] = pd;
            }
            return data;
        }
        public override void LoadSaveData(GenericData data)
        {
            if (data.TryReadData("affixes", out var ad))
            {
                affixes.Clear();
                foreach (var pd in ad.ReadDataDict())
                {
                    var prop = new ItemProperty(ItemAttributeEnum.Instance[pd.Key])
                    {
                        Value = (ValueType)pd.Value["value"]
                    };
                    affixes.Add(prop);
                }
            }
        }
    }
}