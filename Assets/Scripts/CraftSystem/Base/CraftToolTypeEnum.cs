using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ZetanStudio.CraftSystem
{
    [CreateAssetMenu(fileName = "craft tool type", menuName = "Zetan Studio/道具/枚举/工艺工具")]
    public sealed class CraftToolTypeEnum : ScriptableObjectEnum<CraftToolTypeEnum, CraftToolType>
    {
        public CraftToolTypeEnum()
        {
            _enum = new CraftToolType[]
            {
                new CraftToolType("手工", CraftMethodEnum.NameToIndex("手工")),
                new CraftToolType("手工台", CraftMethodEnum.NameToIndex("手工"), CraftMethodEnum.NameToIndex("精手工")),
                new CraftToolType("冶炼炉", CraftMethodEnum.NameToIndex("冶炼")),
                new CraftToolType("锻造台", CraftMethodEnum.NameToIndex("锻造")),
                new CraftToolType("锻冶台", CraftMethodEnum.NameToIndex("冶炼"), CraftMethodEnum.NameToIndex("锻造")),
                new CraftToolType("纺织机", CraftMethodEnum.NameToIndex("纺织")),
                new CraftToolType("缝纫机", CraftMethodEnum.NameToIndex("缝纫")),
                new CraftToolType("灶台", CraftMethodEnum.NameToIndex("烹饪")),
                new CraftToolType("制药台", CraftMethodEnum.NameToIndex("制药")),
                new CraftToolType("晾晒台", CraftMethodEnum.NameToIndex("晾晒")),
                new CraftToolType("臼杵", CraftMethodEnum.NameToIndex("研磨")),
                new CraftToolType("过滤器", CraftMethodEnum.NameToIndex("过滤")),
                new CraftToolType("烤炉", CraftMethodEnum.NameToIndex("烘烤")),
            };
        }
    }

    [System.Serializable]
    public sealed class CraftToolType : ScriptableObjectEnumItem
    {
        [SerializeField, Enum(typeof(CraftMethod))]
        private List<int> methods;
        public ReadOnlyCollection<CraftMethod> Methods => new ReadOnlyCollection<CraftMethod>(methods.ConvertAll(x => CraftMethodEnum.Instance[x]));

        public CraftToolType() : base("手工") { }

        public CraftToolType(string name, params int[] method) : base(name)
        {
            methods = new List<int>(method);
        }
    }
}