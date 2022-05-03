using UnityEngine;

namespace ZetanStudio.Item.Making
{
    [CreateAssetMenu(fileName = "making type", menuName = "Zetan Studio/道具/枚举/制作方法")]
    public class MakingMethodEnum : ScriptableObjectEnum<MakingMethodEnum, MakingMethod>
    {
        public MakingMethodEnum()
        {
            @enum = new MakingMethod[]
            {
                new MakingMethod("手工"),
                new MakingMethod("冶炼"),
                new MakingMethod("锻造"),
                new MakingMethod("纺织"),
                new MakingMethod("缝纫"),
                new MakingMethod("烹饪"),
                new MakingMethod("制药"),
                new MakingMethod("晾晒"),
                new MakingMethod("研磨"),
            };
        }
    }

    [System.Serializable]
    public class MakingMethod : ScriptableObjectEnumItem
    {
        public MakingMethod() : this("手工") { }

        public MakingMethod(string name)
        {
            Name = name;
        }
    }
}