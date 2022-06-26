using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{

    [Name("词缀强化"), Require(typeof(EnhancementModule))]
    public class AffixEnhancementModule : ItemModule
    {
        public override bool IsValid => enhancementInfo && enhancementInfo.IsValid || !enhancementInfo && (enhancement?.IsValid ?? false);

        [SerializeField]
        private AffixEnhancementInfo enhancementInfo;

        [SerializeField]
        private AffixEnhancement enhancement;

        public IEnumerable<ItemProperty> GenerateIncrements(int upperLimit, IEnumerable<ItemProperty> properties, int times)
        {
            if ((enhancement == null || !enhancement.IsValid) && enhancementInfo) return enhancementInfo.GenerateEnhancements(upperLimit, properties, times);
            else return enhancement?.GenerateEnhancements(upperLimit, properties, times) ?? new List<ItemProperty>();
        }
    }
}