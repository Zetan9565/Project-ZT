using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("可制作")]
    public class MakableModule : ItemModule
    {
        [SerializeField, DisplayName("制作方法"), Enum(typeof(Making.MakingMethod))]
        private int makingMethod;
        public Making.MakingMethod MakingMethod => Making.MakingMethodEnum.Instance[makingMethod];

        [SerializeField, DisplayName("可自学")]
        private bool canMakeByTry;
        public bool CanMakeByTry => canMakeByTry && Formulation && !Formulation.Materials.TrueForAll(x => x.MakingType == MakingType.SingleItem);

        [field: SerializeField, DisplayName("配方")]
        public Formulation Formulation { get; protected set; }

        [field: SerializeField]
        public MakingYield[] Yields { get; protected set; } = new MakingYield[]
        {
            new MakingYield(1,1)
        };

        public override bool IsValid => Formulation && Formulation.IsValid;
    }

    [System.Serializable]
    public sealed class MakingYield
    {
        [field: SerializeField, Min(1)]
        public int Yield { get; private set; }

        [field: SerializeField]
        public float Rate { get; private set; }

        public MakingYield()
        {

        }

        public MakingYield(int yield, float rate)
        {
            Yield = yield;
            Rate = rate;
        }
    }
}