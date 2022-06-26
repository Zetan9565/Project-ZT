﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using ZetanStudio.Character;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("属性")]
    public class AttributeModule : ItemModule
    {
        [SerializeField]
        private List<ItemAttribute> attributes;
        public ReadOnlyCollection<ItemAttribute> Attributes => attributes.AsReadOnly();

        public override bool IsValid => !Attributes.Any(x => string.IsNullOrEmpty(x.Name));

        public override ItemModuleData CreateData(ItemData item)
        {
            return new AttributeData(item, this);
        }
    }

    public class AttributeData : ItemModuleData<AttributeModule>
    {
        public List<ItemProperty> properties;

        public AttributeData(ItemData item, AttributeModule module) : base(item, module)
        {
            properties = new List<ItemProperty>();
            foreach (var a in module.Attributes)
            {
                properties.Add(new ItemProperty(a.Type) { Value = a.Value });
            }
        }

        public override SaveDataItem GetSaveData()
        {
            var data = new SaveDataItem();
            List<RoleValueSaveData> saveDatas = new List<RoleValueSaveData>();
            foreach (var prop in properties)
            {
                saveDatas.Add(new RoleValueSaveData(prop));
            }
            data.stringData["properties"] = ZetanUtility.ToJson(saveDatas);
            return data;
        }
        public override void LoadSaveData(SaveDataItem data)
        {
            List<RoleValueSaveData> loadDatas = ZetanUtility.FromJson<List<RoleValueSaveData>>(data.stringData["properties"]);
            foreach (var load in loadDatas)
            {
                if(properties.Find(x=>x.ID == load.ID) is ItemProperty prop)
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

namespace ZetanStudio.ItemSystem
{
    [Serializable]
    public class ItemAttribute : IRoleValue<ItemAttributeType>
    {
        public string Name => string.IsNullOrEmpty(Type.Alias) ? Type.Name : Type.Alias;
        public string ID => Type.ID;

        [SerializeField, Enum(typeof(ItemAttributeType))]
        private int type;
        public ItemAttributeType Type => ItemAttributeEnum.Instance[type];

        public ValueType Value
        {
            get
            {
                switch (Type.ValueType)
                {
                    case RoleValueType.Integer:
                        return IntValue;
                    case RoleValueType.Float:
                        return FloatValue;
                    case RoleValueType.Boolean:
                        return BoolValue;
                    default:
                        throw new InvalidOperationException("意料之外的错误");
                }
            }
        }

        public string ValueString
        {
            get
            {
                return ValueType switch
                {
                    RoleValueType.Integer => string.Format(Type.Format, intValue).Replace(".0%", "%"),
                    RoleValueType.Float => string.Format(Type.Format, floatValue).Replace(".0%", "%"),
                    RoleValueType.Boolean => string.Format(Type.Format, boolValue),
                    _ => throw new InvalidOperationException("意料之外的错误"),
                };
            }
        }

        public RoleValueType ValueType => Type.ValueType;

        public bool HasIntValue => ValueType == RoleValueType.Integer;
        public bool HasFloatValue => ValueType == RoleValueType.Float;
        public bool HasBoolValue => ValueType == RoleValueType.Boolean;

        [SerializeField, ShowIf("ValueType", RoleValueType.Integer)]
        private int intValue;
        public int IntValue
        {
            get
            {
                if (!HasIntValue) Debug.LogWarning($"[{Name}] 属性使用的不是整型数值");
                return intValue;
            }
        }

        [SerializeField, ShowIf("ValueType", RoleValueType.Float)]
        private float floatValue;
        public float FloatValue
        {
            get
            {
                if (!HasFloatValue) Debug.LogWarning($"[{Name}] 属性使用的不是浮点型数值");
                return floatValue;
            }
        }

        [SerializeField, ShowIf("ValueType", RoleValueType.Boolean)]
        private bool boolValue;
        public bool BoolValue
        {
            get
            {
                if (!HasBoolValue) Debug.LogWarning($"[{Name}] 属性使用的不是布尔型数值");
                return boolValue;
            }
        }

        public static implicit operator bool(ItemAttribute self)
        {
            return self != null;
        }
        public override string ToString()
        {
            return $"{Name} +{ValueString}";
        }
    }
}