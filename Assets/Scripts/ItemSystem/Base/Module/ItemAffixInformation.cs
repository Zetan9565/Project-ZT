using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.CharacterSystem;
using ZetanStudio.Math;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "item affix info", menuName = "Zetan Studio/道具/道具词缀")]
    public class ItemAffixInformation : ScriptableObject
    {
        [SerializeField]
        private ItemAffix affix;

        public bool IsValid => affix?.IsValid ?? false;

        public int UpperLimit => affix?.UpperLimit ?? 0;

        public IEnumerable<ItemProperty> GenerateAffixes()
        {
            return affix?.GenerateAffixes() ?? new ItemProperty[0];
        }
    }

    [System.Serializable]
    public class ItemAffix
    {
        [field: SerializeField, Min(1)]
        public int UpperLimit { get; private set; } = 4;

        [SerializeField]
        private Vector2Int affixCountRange = Vector2Int.up;

        [SerializeField]
        private AnimationCurve affixCountDistrib = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        [SerializeField]
        private AnimationCurve affixIndexDistrib = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        [SerializeField]
        private RandomAffix[] affixes;

        public bool IsValid => affixCountRange.x >= 0 && affixCountRange.y > 0 && affixCountDistrib.keys.Length > 0 && affixIndexDistrib.keys.Length > 0 && affixes.Length > 0;

        public bool IsDefinite => affixCountRange.x == affixCountRange.y && affixes.All(x => x.IsDefinite);

        public IEnumerable<ItemProperty> GenerateAffixes()
        {
            List<ItemProperty> results = new List<ItemProperty>(this.affixes.Length);
            if (!IsValid) return results;
            int count = getCount();
            var affixes = new HashSet<string>(this.affixes.Select(x => x.Type.ID));
            while (affixes.Count > 0 && results.Count < count)
            {
                var affix = getAffix();
                if (affix == null) break;
                //如果这种属性从未用过
                if (affixes.Contains(affix.Type.ID))
                {
                    affixes.Remove(affix.Type.ID);//表示用掉了这种属性
                    results.Add(new ItemProperty(affix.Type) { Value = affix.RandomValue() });
                }
            }
            return results;

            int getCount()
            {
                int min = Mathf.Min(affixCountRange.x, affixCountRange.y);
                if (min > this.affixes.Length) return Random.Range(0, this.affixes.Length + 1);
                int max = Mathf.Min(Mathf.Max(affixCountRange.x, affixCountRange.y), UpperLimit, this.affixes.Length);
                if (min == max) return min;
                else return DistributedValue.RangeValue(affixCountDistrib, min, max + 1);
            }
            RandomAffix getAffix()
            {
                if (this.affixes.Length < 1) return null;
                else if (this.affixes.Length == 1) return this.affixes[0];
                else return this.affixes[DistributedValue.RangeValue(affixIndexDistrib, 0, this.affixes.Length)];
            }
        }
    }

    [System.Serializable]
    public class RandomAffix
    {
        [SerializeField, Enum(typeof(ItemAttributeType))]
        private int type;
        public ItemAttributeType Type => ItemAttributeEnum.Instance[type];

        public RoleValueType ValueType => Type.ValueType;

        [SerializeField]
        private Vector2 floatRange;

        [SerializeField]
        private Vector2Int intRange;

        [SerializeField]
        private AnimationCurve valueDistrib = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        [SerializeField, Slider(0, 1)]
        private float trueProbility = 0.5f;

        public bool IsValid
        {
            get
            {
                return ValueType switch
                {
                    RoleValueType.Integer => intRange.x != intRange.y && valueDistrib.length > 1 && valueDistrib.keys.Any(x => x.value > 0) || intRange.x == intRange.y,
                    RoleValueType.Float => floatRange.x != floatRange.y && valueDistrib.length > 1 && valueDistrib.keys.Any(x => x.value > 0) || floatRange.x == floatRange.y,
                    RoleValueType.Boolean => true,
                    _ => throw new System.Exception(),
                };
            }
        }

        public bool IsDefinite
        {
            get
            {
                return ValueType switch
                {
                    RoleValueType.Integer => intRange.x == intRange.y,
                    RoleValueType.Float => floatRange.x == floatRange.y,
                    RoleValueType.Boolean => trueProbility == 0f || trueProbility == 1f,
                    _ => throw new System.Exception(),
                };
            }
        }

        public System.ValueType RandomValue()
        {
            return ValueType switch
            {
                RoleValueType.Integer => RandomInt(),
                RoleValueType.Float => RandomFloat(),
                RoleValueType.Boolean => RandomBool(),
                _ => throw new System.Exception(),
            };
        }
        public int RandomInt()
        {
            return DistributedValue.RangeValue(valueDistrib, intRange.x, intRange.y + 1);
        }
        public float RandomFloat()
        {
            return DistributedValue.RangeValue(valueDistrib, floatRange.x, floatRange.y);
        }
        public bool RandomBool()
        {
            return Random.Range(0f, 1f) <= trueProbility;
        }
    }
}
