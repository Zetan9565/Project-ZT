using UnityEngine;

namespace ZetanStudio.ItemSystem.Craft
{
    [CreateAssetMenu(fileName = "making type", menuName = "Zetan Studio/道具/枚举/工艺类型")]
    public class CraftMethodEnum : ScriptableObjectEnum<CraftMethodEnum, CraftMethod>
    {
        public CraftMethodEnum()
        {
            _enum = new CraftMethod[]
            {
                new CraftMethod("手工"),
                new CraftMethod("精手工"),
                new CraftMethod("冶炼"),
                new CraftMethod("锻造"),
                new CraftMethod("纺织"),
                new CraftMethod("缝纫"),
                new CraftMethod("烹饪"),
                new CraftMethod("制药"),
                new CraftMethod("晾晒"),
                new CraftMethod("研磨"),
                new CraftMethod("过滤"),
                new CraftMethod("烘烤"),
            };
        }
    }

    [System.Serializable]
    public class CraftMethod : ScriptableObjectEnumItem
    {
        public CraftMethod() : this("手工") { }

        public CraftMethod(string name)
        {
            Name = name;
        }
    }
}