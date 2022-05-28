using UnityEngine;

namespace ZetanStudio.Item
{
    [CreateAssetMenu(fileName = "currency type", menuName = "Zetan Studio/道具/枚举/货币类型")]
    public sealed class CurrencyTypeEnum : ScriptableObjectEnum<CurrencyTypeEnum, CurrencyType>
    {
        public CurrencyTypeEnum()
        {
            _enum = new CurrencyType[]
            {
                new CurrencyType("金币"),
                new CurrencyType("经验"),
            };
        }
    }

    [System.Serializable]
    public sealed class CurrencyType : ScriptableObjectEnumItem
    {
        public CurrencyType()
        {
            Name = "金币";
        }

        public CurrencyType(string name)
        {
            Name = name;
        }
    }
}