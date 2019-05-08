using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private static TimeManager instance;
    public static TimeManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<TimeManager>();
            return instance;
        }
    }

    public const float OneGameMinute = 2.5f;
    public const float OneGameHour = 150.0f;
    public const float OneGameDay = 3600.0f;
}
