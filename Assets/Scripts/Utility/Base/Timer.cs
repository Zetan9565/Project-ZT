using System;
using UnityEngine;
using ZetanStudio;

/// <summary>
/// 递增计时器
/// </summary>
public class Timer
{
    public float TargetTime { get; private set; }
    public int TargetInvokeTimes { get; private set; }
    public float Time { get; private set; }
    public float DeltaTime { get; private set; }
    public int InvokeTimes { get; private set; }
    public bool IsStop { get; private set; }

#pragma warning disable IDE1006 // 命名样式
    public event Action callback;
    public event Action<float> callback_time;
    public event Action<Timer> callback_transfer;
    private readonly bool loop;
    public event Action<int> callback_loop;
#pragma warning restore IDE1006 // 命名样式

    private readonly bool ignoreTimeScale;

    /// <summary>
    /// 只回调一次的简易计时器
    /// </summary>
    /// <param name="callback">回调动作</param>
    /// <param name="time">回调延时</param>
    private Timer(Action callback, float time, bool ignoreTimeScale = false)
    {
        this.callback = callback;
        TargetTime = time;
        this.ignoreTimeScale = ignoreTimeScale;
        Restart();
    }
    /// <summary>
    /// 可访问计时器本体的计时器
    /// </summary>
    /// <param name="callback">回调动作</param>
    /// <param name="times">回调次数</param>
    /// <param name="time">回调间隔</param>
    private Timer(Action<Timer> callback, int times, float time, bool ignoreTimeScale = false)
    {
        callback_transfer = callback;
        TargetTime = time;
        TargetInvokeTimes = times;
        loop = times != 0;
        this.ignoreTimeScale = ignoreTimeScale;
        Restart();
    }

    /// <summary>
    /// 可访问循环次数的计时器
    /// </summary>
    /// <param name="callback">回调动作</param>
    /// <param name="times">回调次数</param>
    /// <param name="time">回调间隔</param>
    private Timer(Action<int> callback, int times, float time, bool ignoreTimeScale = false)
    {
        callback_loop = callback;
        TargetTime = time;
        TargetInvokeTimes = times;
        loop = times != 0;
        this.ignoreTimeScale = ignoreTimeScale;
        Restart();
    }
    /// <summary>
    /// 可访问计时读数的计时器
    /// </summary>
    /// <param name="callback">回调动作</param>
    /// <param name="times">回调次数</param>
    /// <param name="time">回调间隔</param>
    private Timer(Action<float> callback, float time, bool ignoreTimeScale = false)
    {
        callback_time = callback;
        TargetTime = time;
        this.ignoreTimeScale = ignoreTimeScale;
        Restart();
    }

    private void Update(float time)
    {
        if (!IsStop)
        {
            Time += time;
            DeltaTime = time;
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

    public void Restart(float from = 0)
    {
        IsStop = false;
        Time = from;
        InvokeTimes = 0;
        if (ignoreTimeScale)
        {
            realTimeCallback -= Update;
            realTimeCallback += Update;
        }
        else
        {
            updateCallback -= Update;
            updateCallback += Update;
        }
        Update(0);
    }

    public void Stop()
    {
        IsStop = true;
        updateCallback -= Update;
        realTimeCallback -= Update;
    }

    #region 静态成员
    private static Action<float> updateCallback;
    private static Action<float> realTimeCallback;

    [RuntimeInitializeOnLoadMethod]
    private static void Register()
    {
        EmptyMonoBehaviour.Singleton.UpdateCallback -= Update;
        EmptyMonoBehaviour.Singleton.UpdateCallback += Update;
    }

    private static void Update()
    {
        updateCallback?.Invoke(UnityEngine.Time.deltaTime);
        realTimeCallback?.Invoke(UnityEngine.Time.unscaledDeltaTime);
    }

    /// <summary>
    /// 创建简单计时器
    /// </summary>
    /// <param name="callback">计时回调</param>
    /// <param name="time">时间</param>
    /// <param name="ignoreTimeScale">忽略时间缩放</param>
    /// <returns>创建的计时器</returns>
    public static Timer Create(Action callback, float time, bool ignoreTimeScale = false)
    {
        if (time < 0) return null;
        Timer timer = new Timer(callback, time, ignoreTimeScale);
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
    public static Timer Create(Action<int> callback, float time, int times, bool ignoreTimeScale = false)
    {
        if (time < 0 || times == 0) return null;
        Timer timer = new Timer(callback, times, time, ignoreTimeScale);
        return timer;
    }

    /// <summary>
    /// 创建读数计时器
    /// </summary>
    /// <param name="callback">传入计时读数的回调</param>
    /// <param name="time">时间</param>
    /// <param name="ignoreTimeScale">忽略时间缩放</param>
    /// <returns>创建的计时器</returns>
    public static Timer Create(Action<float> callback, float time, bool ignoreTimeScale = false)
    {
        if (time < 0) return null;
        Timer timer = new Timer(callback, time, ignoreTimeScale);
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
    public static Timer Create(Action<Timer> callback, float time, int times, bool ignoreTimeScale = false)
    {
        if (time < 0 || times == 0) return null;
        Timer timer = new Timer(callback, times, time, ignoreTimeScale);
        return timer;
    }
    #endregion
}