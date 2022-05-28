using UnityEngine;
using ZetanStudio.Item.Craft;

namespace ZetanStudio.Item.Module
{
    [Name("可制作")]
    public class CraftableModule : ItemModule
    {
        [SerializeField, Label("制作方法"), Enum(typeof(CraftMethod))]
        private int craftMethod;
        public CraftMethod CraftMethod => CraftMethodEnum.Instance[craftMethod];

        [SerializeField, Label("可自学")]
        private bool canMakeByTry;
        public bool CanMakeByTry => canMakeByTry && Formulation && !Formulation.Materials.TrueForAll(x => x.MakingType == CraftType.SingleItem);

        [field: SerializeField, Label("配方"), ObjectSelector("ToString")]
        public Formulation Formulation { get; protected set; }

        [field: SerializeField]
        public CraftYield[] Yields { get; protected set; } = new CraftYield[]
        {
            new CraftYield(1,1)
        };

        public int RandomAmount()
        {
            float random = Random.Range(0, 1f);
            foreach (var yield in Yields)
            {
                if (random <= yield.Rate)
                    return yield.Amount;
            }
            return 1;
        }

        public override bool IsValid => Formulation && Formulation.IsValid;
    }

    [System.Serializable]
    public sealed class CraftYield
    {
        [field: SerializeField, Min(1)]
        public int Amount { get; private set; }

        [field: SerializeField]
        public float Rate { get; private set; }

        public CraftYield()
        {

        }

        public CraftYield(int amount, float rate)
        {
            Amount = amount;
            Rate = rate;
        }
    }
}