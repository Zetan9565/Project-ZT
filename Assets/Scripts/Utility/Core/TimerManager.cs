using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class TimerManager : SingletonMonoBehaviour<TimerManager>
{
    private readonly List<Timer> timers = new List<Timer>();
    private readonly List<Timer> realTimers = new List<Timer>();

    private void Awake()
    {
        StartCoroutine(UpdateRealtime());
    }

    private void LateUpdate()
    {
        timers.RemoveAll(x => x.IsStop);
        using var timerEnum = timers.GetEnumerator();
        while (timerEnum.MoveNext())
        {
            timerEnum.Current.Update(Time.deltaTime);
        }
    }

    private IEnumerator UpdateRealtime()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            realTimers.RemoveAll(x => x.IsStop);
            using var timerEnum = realTimers.GetEnumerator();
            while (timerEnum.MoveNext())
            {
                timerEnum.Current.Update(Time.unscaledDeltaTime);
            }
        }
    }

    /// <summary>
    /// 创建简单计时器
    /// </summary>
    /// <param name="callback">计时回调</param>
    /// <param name="time">时间</param>
    /// <param name="ignoreTimeScale">忽略时间缩放</param>
    /// <returns>创建的计时器</returns>
    public Timer Create(Action callback, float time, bool ignoreTimeScale = false)
    {
        if (time < 0) return null;
        Timer timer = new Timer(callback, time);
        Insert(timer, ignoreTimeScale);
        return timer;
    }

    /// <summary>
    /// 创建循环计时器
    /// </summary>
    /// <param name="callback">传入执行次数的回调</param>
    /// <param name="times">执行次数，小于0时循环执行</param>
    /// <param name="time">时间</param>
    /// <param name="ignoreTimeScale">忽略时间缩放</param>
    /// <returns>创建的计时器</returns>
    public Timer Create(Action<int> callback, int times, float time, bool ignoreTimeScale = false)
    {
        if (time < 0 || times == 0) return null;
        Timer timer = new Timer(callback, times, time);
        Insert(timer, ignoreTimeScale);
        return timer;
    }

    /// <summary>
    /// 创建通用计时器
    /// </summary>
    /// <param name="callback">传入计算器的回调</param>
    /// <param name="times">执行次数，小于0时循环执行</param>
    /// <param name="time">时间</param>
    /// <param name="ignoreTimeScale">忽略时间缩放</param>
    /// <returns>创建的计算器</returns>
    public Timer Create(Action<Timer> callback, int times, float time, bool ignoreTimeScale = false)
    {
        if (time < 0 || times == 0) return null;
        Timer timer = new Timer(callback, times, time);
        Insert(timer, ignoreTimeScale);
        return timer;
    }

    private void Insert(Timer timer, bool ignoreTimeScale)
    {
        if (ignoreTimeScale) realTimers.Add(timer);
        else timers.Add(timer);
    }

}
public class Timer
{
    private readonly float targetTime;
    private readonly Action callback;
    private readonly Action<Timer> callback_transfer;

    private readonly Action<int> callback_loop;
    private readonly bool loop;
    private readonly int targetTimes;

    private float currentTime;
    public float CurrentTime => currentTime;

    private int times;
    public int Times => times;

    private bool isStop;
    public bool IsStop => isStop;

    public Timer(Action callback, float time)
    {
        this.callback = callback;
        targetTime = time;
    }
    public Timer(Action<Timer> callback, int times, float time)
    {
        callback_transfer = callback;
        targetTime = time;
        targetTimes = times;
        loop = times != 0;
    }

    public Timer(Action<int> callback, int times, float time)
    {
        callback_loop = callback;
        targetTime = time;
        targetTimes = times;
        loop = times != 0;
    }

    public void Update(float time)
    {
        if (!isStop)
        {
            currentTime += time;
            if (currentTime >= targetTime)
            {
                if (loop)
                {
                    times++;
                    callback_loop?.Invoke(times);
                    callback_transfer?.Invoke(this);
                    currentTime -= targetTime;
                    if (targetTimes > 0 && times >= targetTimes) Stop();
                }
                else
                {
                    callback?.Invoke();
                    callback_transfer?.Invoke(this);
                    Stop();
                }
            }
        }
    }

    public void Stop()
    {
        isStop = true;
    }
}