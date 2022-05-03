using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("药品"), Require(typeof(UsableModule))]
    public class MedichineModule : ItemModule
    {
        public override bool IsValid => true;
    }
}