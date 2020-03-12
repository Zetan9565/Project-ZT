using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DateAgent : MonoBehaviour
{
    public Text dateText;
    public int date;
    public Color todayColor = new Color(0.39f, 0.78f, 0.86f, 1);
    public void Init(int daysOfMonth, Color textColor)
    {
        date = daysOfMonth;
        string dayString = daysOfMonth.ToString();
        if (TimeManager.Instance.TimeSystem == TimeSystem.Twelve)
        {
            int dayOfDate = daysOfMonth;
            if (dayOfDate < 20)
                switch (dayOfDate)
                {
                    case 1: dayString = "初一"; break;
                    case 2: dayString = "初二"; break;
                    case 3: dayString = "初三"; break;
                    case 4: dayString = "初四"; break;
                    case 5: dayString = "初五"; break;
                    case 6: dayString = "初六"; break;
                    case 7: dayString = "初七"; break;
                    case 8: dayString = "初八"; break;
                    case 9: dayString = "初九"; break;
                    case 10: dayString = "初十"; break;
                    case 11: dayString = "十一"; break;
                    case 12: dayString = "十二"; break;
                    case 13: dayString = "十三"; break;
                    case 14: dayString = "十四"; break;
                    case 15: dayString = "十五"; break;
                    case 16: dayString = "十六"; break;
                    case 17: dayString = "十七"; break;
                    case 18: dayString = "十八"; break;
                    case 19: dayString = "十九"; break;
                    default: dayString = string.Empty; break;
                }
            else if (dayOfDate >= 20 && dayOfDate < 30)
                switch (dayOfDate)
                {
                    case 20: dayString = "廿十"; break;
                    case 21: dayString = "廿一"; break;
                    case 22: dayString = "廿二"; break;
                    case 23: dayString = "廿三"; break;
                    case 24: dayString = "廿四"; break;
                    case 25: dayString = "廿五"; break;
                    case 26: dayString = "廿六"; break;
                    case 27: dayString = "廿七"; break;
                    case 28: dayString = "廿八"; break;
                    case 29: dayString = "廿九"; break;
                    default: dayString = string.Empty; break;
                }
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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
