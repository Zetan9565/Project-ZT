using UnityEngine;

namespace ZetanStudio
{
    public static class MiscFuntion
    {
        public static string GetColorAmountString(int current, int target, Color? enough = null, Color? lack = null)
        {
            return $"{Utility.ColorText(current, current >= target ? enough ?? Color.green : lack ?? Color.red)}/{target}";
        }

        public static string ToChineseSortNum(long value)
        {
            decimal temp = value / 100000000m;
            if (temp >= 1)
            {
                return $"{temp:#0.#}亿";
            }
            temp = value / 10000m;
            if (temp >= 1)
            {
                return $"{temp:#0.#}万";
            }
            return value.ToString();
        }

        public static string SecondsToSortTime(float seconds, string dayStr = "天", string hourStr = "时", string minuStr = "分", string secStr = "秒")
        {
            if (!string.IsNullOrEmpty(dayStr))
            {
                const float day = 86400f;
                if (seconds >= day) return Mathf.CeilToInt(seconds / day) + dayStr;
            }
            if (!string.IsNullOrEmpty(hourStr))
            {
                const float hour = 3600f;
                if (seconds >= hour) return Mathf.CeilToInt(seconds / hour) + hourStr;
            }
            if (!string.IsNullOrEmpty(minuStr))
            {
                const float minute = 60f;
                if (seconds >= minute) return Mathf.CeilToInt(seconds / minute) + minuStr;
            }
            return Mathf.CeilToInt(seconds) + secStr;
        }
    }
}