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

    private static readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    private IEnumerator UpdateRealtime()
    {
        while (true)
        {
            yield return waitForEndOfFrame;
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
    public Timer Create(Action<int> callback, float time, int times, bool ignoreTimeScale = false)
    {
        if (time < 0 || times == 0) return null;
        Timer timer = new Timer(callback, times, time);
        Insert(timer, ignoreTimeScale);
        return timer;
    }

    /// <summary>
    /// 创建读数计时器
    /// </summary>
    /// <param name="callback">传入计时读数的回调</param>
    /// <param name="time">时间</param>
    /// <param name="ignoreTimeScale">忽略时间缩放</param>
    /// <returns>创建的计时器</returns>
    public Timer Create(Action<float> callback, float time, bool ignoreTimeScale = false)
    {
        if (time < 0) return null;
        Timer timer = new Timer(callback, time);
        Insert(timer, ignoreTimeScale);
        return timer;
    }

    /// <summary>
    /// 创建通用计时器
    /// </summary>
    /// <param name="callback">传入计时器的回调</param>
    /// <param name="times">执行次数，小于0时循环执行</param>
    /// <param name="time">时间</param>
    /// <param name="ignoreTimeScale">忽略时间缩放</param>
    /// <returns>创建的计算器</returns>
    public Timer Create(Action<Timer> callback, float time, int times, bool ignoreTimeScale = false)
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
    public float TargetTime { get; }
    public int TargetInvokeTimes { get; }
    public float Time { get; private set; }
    public int InvokeTimes { get; private set; }
    public bool IsStop { get; private set; }

    private readonly Action callback;
    private readonly Action<float> callback_time;
    private readonly Action<Timer> callback_transfer;
    private readonly bool loop;
    private readonly Action<int> callback_loop;

    /// <summary>
    /// 只回调一次的简易计时器
    /// </summary>
    /// <param name="callback">回调动作</param>
    /// <param name="time">回调延时</param>
    public Timer(Action callback, float time)
    {
        this.callback = callback;
        TargetTime = time;
    }
    /// <summary>
    /// 可访问计时器本体的计时器
    /// </summary>
    /// <param name="callback">回调动作</param>
    /// <param name="times">回调次数</param>
    /// <param name="time">回调间隔</param>
    public Timer(Action<Timer> callback, int times, float time)
    {
        callback_transfer = callback;
        TargetTime = time;
        TargetInvokeTimes = times;
        loop = times != 0;
    }

    /// <summary>
    /// 可访问循环次数的计时器
    /// </summary>
    /// <param name="callback">回调动作</param>
    /// <param name="times">回调次数</param>
    /// <param name="time">回调间隔</param>
    public Timer(Action<int> callback, int times, float time)
    {
        callback_loop = callback;
        TargetTime = time;
        TargetInvokeTimes = times;
        loop = times != 0;
    }
    /// <summary>
    /// 可访问计时读数的计时器
    /// </summary>
    /// <param name="callback">回调动作</param>
    /// <param name="times">回调次数</param>
    /// <param name="time">回调间隔</param>
    public Timer(Action<float> callback, float time)
    {
        callback_time = callback;
        TargetTime = time;
    }

    public void Update(float time)
    {
        if (!IsStop)
        {
            Time += time;
            if (Time >= TargetTime)
            {
                if (loop)
                {
                    InvokeTimes++;
                    callback_loop?.Invoke(InvokeTimes);
                    if (TargetInvokeTimes > 0 && InvokeTimes >= TargetInvokeTimes) Stop();
                    else Time -= TargetTime;
                }
                else
                {
                    Stop();
                    callback?.Invoke();
                }
            }
            callback_time?.Invoke(Time);
            callback_transfer?.Invoke(this);
        }
    }

    public void Stop()
    {
        IsStop = true;
    }
}