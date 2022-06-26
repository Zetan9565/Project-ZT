using System.Linq;
using System.Collections.ObjectModel;
using UnityEngine;
using ZetanStudio.ItemSystem.Craft;
using ZetanStudio.Math;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("可制作")]
    public class CraftableModule : ItemModule
    {
        [SerializeField, Enum(typeof(CraftMethod))]
        private int craftMethod;
        public CraftMethod CraftMethod => CraftMethodEnum.Instance[craftMethod];

        [SerializeField]
        private bool canMakeByTry;
        public bool CanMakeByTry => canMakeByTry && formulation && formulation.Materials.TrueForAll(x => x.CostType == MaterialCostType.SingleItem);

        [SerializeField, ObjectSelector("ToString", displayNone: true, displayAdd: true)]
        private Formulation formulation;

        [SerializeField]
        private MaterialInfo[] materials = { };

        public ReadOnlyCollection<MaterialInfo> Materials => new ReadOnlyCollection<MaterialInfo>(formulation && materials.Length < 1 ? formulation.Materials : materials);

        [field: SerializeField, DistributedValueRange(1, 1)]
        public DistributedIntValue Yield { get; protected set; } = new DistributedIntValue();

        public int RandomAmount() => Yield.RandomValue();

        public override bool IsValid => (materials.Length < 1 && formulation && formulation.IsValid || materials.Length > 0 && materials.All(x => x.IsValid)) && Yield.IsValid;
    }
}