using System;
using System.Linq;
using UnityEngine;

namespace ZetanStudio.Math
{
    public abstract class DistributedValue
    {
        /// <summary>
        /// 按概率分布图从浮点数范围中取样
        /// </summary>
        /// <param name="distribution">概率分布图</param>
        /// <param name="minInclusive">下限</param>
        /// <param name="maxInclusive">上限</param>
        /// <returns>取样值</returns>
        public static float RangeValue(AnimationCurve distribution, float minInclusive, float maxInclusive)
        {
            if (distribution is null) throw new ArgumentNullException(nameof(distribution));
            if (distribution.length < 2) throw new ArgumentException($"{nameof(distribution)}至少要有两个关键帧");
            if (distribution.keys.Max(x => x.value) <= 0) throw new ArgumentException($"{nameof(distribution)}至少要有一个关键帧大于0");
            if (minInclusive == maxInclusive) return minInclusive;
            float range = Mathf.Abs(maxInclusive - minInclusive);
            float position = random();
            while (range > 0 && !ZetanUtility.Probability(distribution.Evaluate(position)))
            {
                position = random();
            }
            return minInclusive + position * range;

            static float random()
            {
                return UnityEngine.Random.Range(0, 1f);
            }
        }
        /// <summary>
        /// 按概率分布图从整型数范围中取样
        /// </summary>
        /// <param name="distribution">概率分布图</param>
        /// <param name="minInclusive">下限</param>
        /// <param name="maxExclusive">上限</param>
        /// <returns>取样值</returns>
        public static int RangeValue(AnimationCurve distribution, int minInclusive, int maxExclusive)
        {
            return Mathf.RoundToInt(RangeValue(distribution, (float)minInclusive, maxExclusive - 1));
        }
    }

    [Serializable]
    public sealed class DistributedIntValue : DistributedValue
    {
        [field: SerializeField]
        public Vector2Int Range { get; private set; } = Vector2Int.one;

        [field: SerializeField]
        public AnimationCurve Distribution { get; private set; } = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        public bool IsValid => Mathf.Max(Range.x, Range.y) > 0 && Distribution.length > 1 && Distribution.keys.Any(x => x.value > 0);

        public bool IsDefinite => Range.x == Range.y;

        public int RandomValue()
        {
            return RangeValue(Distribution, Range.x, Range.y + 1);
        }
    }

    [Serializable]
    public sealed class DistributedFloatValue : DistributedValue
    {
        [field: SerializeField]
        public Vector2 Range { get; private set; } = Vector2.one;

        [field: SerializeField]
        public AnimationCurve Distribution { get; private set; } = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        public bool IsValid => Mathf.Max(Range.x, Range.y) > 0 && Distribution.length > 1 && Distribution.keys.Any(x => x.value > 0);

        public bool IsDefinite => Range.x == Range.y;

        public float RandomValue()
        {
            return RangeValue(Distribution, Range.x, Range.y);
        }
    }

    public sealed class DistributedValueRangeAttribute : Attribute
    {
        public readonly float min;
        public readonly float max;

        public DistributedValueRangeAttribute(float minMin, float minMax)
        {
            min = minMin;
            max = minMax;
        }
    }
}