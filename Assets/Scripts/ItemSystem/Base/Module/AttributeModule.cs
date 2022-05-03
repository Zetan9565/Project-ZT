using UnityEngine;

namespace ZetanStudio.Item.Module
{
    [Name("属性")]
    public class AttributeModule : ItemModule
    {
        [field: SerializeField]
        public RoleAttributeGroup Attributes { get; protected set; }

        public override bool IsValid => !Attributes.Attributes.Exists(x => string.IsNullOrEmpty(x.name));
    }
}