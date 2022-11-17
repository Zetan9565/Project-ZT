using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.UI;

namespace ZetanStudio.TimeSystem.UI
{
    [RequireComponent(typeof(Image))]
    public class DateAgent : GridItem<DateAgent, (int day, Color color)>
    {
        public Text dateText;
        public int date;
        public Color todayColor = new Color(0.39f, 0.78f, 0.86f, 1);
        public void Init(int daysOfMonth, Color textColor)
        {
            date = daysOfMonth;
            string dayString = daysOfMonth.ToString();
            if (TimeManager.Instance.ClockSystem == ClockSystem.Twelve)
            {
                int dayOfDate = daysOfMonth;
                if (dayOfDate < 20)
                    dayString = dayOfDate switch
                    {
                        1 => "初一",
                        2 => "初二",
                        3 => "初三",
                        4 => "初四",
                        5 => "初五",
                        6 => "初六",
                        7 => "初七",
                        8 => "初八",
                        9 => "初九",
                        10 => "初十",
                        11 => "十一",
                        12 => "十二",
                        13 => "十三",
                        14 => "十四",
                        15 => "十五",
                        16 => "十六",
                        17 => "十七",
                        18 => "十八",
                        19 => "十九",
                        _ => string.Empty,
                    };
                else if (dayOfDate >= 20 && dayOfDate < 30)
                    dayString = dayOfDate switch
                    {
                        20 => "廿十",
                        21 => "廿一",
                        22 => "廿二",
                        23 => "廿三",
                        24 => "廿四",
                        25 => "廿五",
                        26 => "廿六",
                        27 => "廿七",
                        28 => "廿八",
                        29 => "廿九",
                        _ => string.Empty,
                    };
                else if (dayOfDate == 30) dayString = "三十";
            }
            dateText.text = dayString;
            dateText.color = textColor;
        }

        public void Empty()
        {
            date = 0;
            dateText.text = string.Empty;
        }

        public void SetToday(bool today)
        {
            GetComponent<Image>().color = today ? todayColor : Color.white; ;
        }

        protected override void RefreshSelected()
        {
            GetComponent<Image>().color = isSelected ? todayColor : Color.white;
        }

        public override void Refresh()
        {
            Init(Data.day, Data.color);
        }
    }
}