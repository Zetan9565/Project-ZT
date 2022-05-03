using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("货币")]
    public sealed class CurrencyModule : ItemModule
    {
        [SerializeField, Enum(typeof(CurrencyType))]
        private int type;
        public CurrencyType Type => CurrencyTypeEnum.Instance[type];

        [field: SerializeField, DisplayName("面额")]
        public int ValueEach { get; private set; }

        public override bool IsValid => ValueEach > 0 && type > -1;
    }
}