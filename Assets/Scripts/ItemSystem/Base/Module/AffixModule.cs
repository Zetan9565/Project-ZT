using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.Character;

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
        public List<ItemProperty> affixes;

        public AffixData(ItemData item, AffixModule module) : base(item, module)
        {
            affixes = new List<ItemProperty>(module.GenerateAffixes());
        }

        public override SaveDataItem GetSaveData()
        {
            var data = new SaveDataItem();
            List<RoleValueSaveData> saveDatas = new List<RoleValueSaveData>();
            foreach (var prop in affixes)
            {
                saveDatas.Add(new RoleValueSaveData(prop));
            }
            data.stringData["affixes"] = ZetanUtility.ToJson(saveDatas);
            return data;
        }
        public override void LoadSaveData(SaveDataItem data)
        {
            List<RoleValueSaveData> loadDatas = ZetanUtility.FromJson<List<RoleValueSaveData>>(data.stringData["affixes"]);
            foreach (var load in loadDatas)
            {
                if (affixes.Find(x => x.ID == load.ID) is ItemProperty prop)
                    switch (prop.ValueType)
                    {
                        case RoleValueType.Integer:
                            prop.IntValue = load.intValue;
                            break;
                        case RoleValueType.Float:
                            prop.FloatValue = load.floatValue;
                            break;
                        case RoleValueType.Boolean:
                            prop.BoolValue = load.boolValue;
                            break;
                    }
            }
        }
    }
}