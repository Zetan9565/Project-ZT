using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    using Character;
    using ZetanStudio.Math;

    [CreateAssetMenu(fileName = "affix enhancement info", menuName = "Zetan Studio/道具/词缀强化信息")]
    public class AffixEnhancementInfo : ScriptableObject
    {
        [SerializeField]
        private AffixEnhancement enhancement;

        public bool IsValid => enhancement?.IsValid ?? false;

        public bool IsDefinite => enhancement?.IsDefinite ?? false;

        public IEnumerable<ItemProperty> GenerateEnhancements(int upperLimit, IEnumerable<ItemProperty> properties, int times)
        {
            return enhancement?.GenerateEnhancements(upperLimit, properties, times) ?? new ItemProperty[0];
        }
    }

    [System.Serializable]
    public class AffixEnhancement
    {
        [SerializeField]
        private Vector2Int affixCountRange = Vector2Int.up;

        [SerializeField]
        private AnimationCurve affixCountDistrib = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        [SerializeField]
        private AnimationCurve affixIndexDistrib = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        [SerializeField]
        private RandomAffix[] affixes;

        public bool IsValid => affixCountRange.x >= 0 && affixCountRange.y > 0 && affixCountDistrib.keys.Length > 0 && affixIndexDistrib.keys.Length > 0 && affixes.Length > 0;

        public bool IsDefinite => affixCountRange.x == affixCountRange.y && affixes.Length == affixCountRange.x && affixes.All(x => x.IsDefinite);

        public IEnumerable<ItemProperty> GenerateEnhancements(int upperLimit, IEnumerable<ItemProperty> properties, int times)
        {
            Dictionary<string, ItemProperty> results = new Dictionary<string, ItemProperty>(affixes.Length);
            if (upperLimit < 1 || !IsValid) return results.Values;
            int need = upperLimit - properties.Count();//需要补全的词缀数量
            var usableType = affixes.Select(x => x.Type.ID);//可用于强化的属性类型集合
            var existType = new HashSet<string>(properties.Select(x => x.Type.ID));//已存在的属性类型
            var newType = new HashSet<string>();//新增的属性类型
            for (int i = 0; i < times; i++)
            {
                var affixes = new HashSet<RandomAffix>(this.affixes);
                var types = new HashSet<string>(usableType);
                int count = getCount();
                while (affixes.Count > 0 && count > 0 && !shoulBreak())
                {
                    var affix = getAffix();
                    if (affix == null) break;
                    bool replenish = need > 0;
                    bool contains = existType.Contains(affix.Type.ID) || newType.Contains(affix.Type.ID);
                    //如果这种属性从未用过，且需要进行补全或无需补全但已存在同类属性
                    if (affixes.Contains(affix) && (replenish && !contains || !replenish && contains))
                    {
                        if (replenish) newType.Add(affix.Type.ID);
                        need--; count--;
                        affixes.Remove(affix); types.Remove(affix.Type.ID);//表示用掉了这种属性
                        if (results.TryGetValue(affix.Type.ID, out var find)) find.Plus(affix.RandomValue());
                        else results.Add(affix.Type.ID, new ItemProperty(affix.Type) { Value = affix.RandomValue() });
                    }
                }

                bool shoulBreak()
                {
                    return need > 0 && types.Count(x => !existType.Contains(x) && !newType.Contains(x)) < 1 ||//如果仍需补全，但剩余的属性(即：不在已有属性列表里的属性)不足以继续补全
                        need <= 0 && !(types.IsSupersetOf(existType) || newType.Count > 0 && types.IsSupersetOf(newType));//如果无需补全，但剩余的属性不包含已有属性或新增属性
                }
            }
            return results.Values;

            int getCount()
            {
                int min = Mathf.Min(affixCountRange.x, affixCountRange.y);
                if (min > affixes.Length) return Random.Range(0, affixes.Length + 1);
                int max = Mathf.Min(Mathf.Max(affixCountRange.x, affixCountRange.y), upperLimit, affixes.Length);
                if (min == max) return min;
                else return DistributedValue.RangeValue(affixCountDistrib, min, max + 1);
            }
            RandomAffix getAffix()
            {
                if (affixes.Length < 0) return null;
                else return affixes[DistributedValue.RangeValue(affixIndexDistrib, 0, affixes.Length)];
            }
        }
    }
}