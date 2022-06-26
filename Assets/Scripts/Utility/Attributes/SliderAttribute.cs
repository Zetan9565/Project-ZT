using UnityEngine;

namespace ZetanStudio
{
    public class SliderAttribute : PropertyAttribute
    {
        public readonly float min;
        public readonly float max;

        public SliderAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
