using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("触发器"), Require(typeof(UsableModule))]
    public class TriggerModule : ItemModule
    {
        [field: SerializeField]
        public string Name { get; private set; }

        [field: SerializeField]
        public bool State { get; private set; }

        public override bool IsValid => !string.IsNullOrEmpty(Name);
    }
}