using UnityEngine;

public class TimeCost
{
    [SerializeField]
    private int value;
    public int Value => value;

    [SerializeField]
    private TimeUnit unit;
    public TimeUnit Unit => unit;

    public decimal ToRealTime()
    {
        if (TimeManager.Instance)
        {
            return unit switch
            {
                TimeUnit.Minute => (decimal)TimeManager.Instance.ScaleMinuteToReal * value,
                TimeUnit.Hour => (decimal)TimeManager.Instance.ScaleHourToReal * value,
                TimeUnit.Day => (decimal)TimeManager.Instance.ScaleDayToReal * value,
                TimeUnit.Month => (decimal)TimeManager.Instance.ScaleMonthToReal * value,
                TimeUnit.Season => (decimal)TimeManager.Instance.ScaleSeasonToReal * value,
                TimeUnit.Year => (decimal)TimeManager.Instance.ScaleYearToReal * value,
                _ => value,
            };
        }
        else return value;
    }
}