using UnityEngine;

namespace ZetanStudio.ItemSystem
{
    [CreateAssetMenu(fileName = "material type", menuName = "Zetan Studio/道具/枚举/材料类型")]
    public sealed class MaterialTypeEnum : ScriptableObjectEnum<MaterialTypeEnum, MaterialType>
    {
        public MaterialTypeEnum()
        {
            _enum = new MaterialType[]
            {
                new MaterialType("矿石"),
                new MaterialType("金属"),
                new MaterialType("纤维"),
                new MaterialType("蔬菜"),
                new MaterialType("药材"),
                new MaterialType("木材"),
                new MaterialType("布料"),
                new MaterialType("兽肉"),
                new MaterialType("禽肉"),
                new MaterialType("鱼肉"),
                new MaterialType("兽毛"),
                new MaterialType("羽毛"),
                new MaterialType("果实"),
                new MaterialType("液体"),
                new MaterialType("调料"),
            };
        }
    }

    [System.Serializable]
    public sealed class MaterialType : ScriptableObjectEnumItem
    {
        [SerializeField]
        public Sprite Icon { get; private set; }

        public MaterialType()
        {
            Name = "未定义";
        }

        public MaterialType(string name)
        {
            Name = name;
        }
    }
}
