using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [Name("强化经验值")]
    public class EnhExperienceModule : ItemModule
    {
        public override bool IsValid => Experience > 0;

        [field: SerializeField]
        public int Type { get; private set; }

        [field: SerializeField, Min(1)]
        public int Experience { get; private set; } = 1;
    }
}