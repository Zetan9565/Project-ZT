using System;
using System.Collections;
using UnityEngine;

public interface IWindowHandler
{
    /// <summary>
    /// 需在这个方法中把自身入栈到WindowsManager的窗口栈中
    /// </summary>
    void OpenWindow();

    /// <summary>
    /// 需在这个方法中把自身从WindowsManager的窗口栈中出栈
    /// </summary>
    void CloseWindow();

    /// <summary>
    /// 需在这个方法中对IsPausing进行处理
    /// </summary>
    /// <param name="state"></param>
    void PauseDisplay(bool pause);

    /// <summary>
    /// 窗口是否打开
    /// </summary>
    bool IsUIOpen { get; }

    /// <summary>
    /// 窗口是否暂停显示
    /// </summary>
    bool IsPausing { get; }

    Canvas CanvasToSort { get; }
}

public interface IHideable
{
    bool IsHidden { get; }
    /// <summary>
    /// 显隐窗口，不应单独使用，显隐窗口请使用<see cref="NewWindowsManager.HideWindow(string, bool, object[])"/>
    /// 、<see cref="NewWindowsManager.HideWindow{T}(bool, object[])"/>、<see cref="NewWindowsManager.HideWindow(IHideable, bool, object[])"/>
    /// </summary>
    /// <param name="hide">是否隐藏</param>
    /// <param name="args">变长参数</param>
    void Hide(bool hide, params object[] args);

    public static void HideHelper(CanvasGroup canvas, bool hide)
    {
        canvas.alpha = hide ? 0 : 1;
        canvas.blocksRaycasts = !hide;
    }
}

public interface IOpenCloseAbleWindow
{
    void OpenCloseWindow();
}

public interface IFadeAble<T> where T : MonoBehaviour
{
    T MonoBehaviour { get; }

    CanvasGroup FadeTarget { get; }

    Coroutine FadeCoroutine { get; set; }

    static void FadeTo(IFadeAble<T> fader, float alpha, float duration, Action onDone = null)
    {
        if (!fader.FadeTarget) return;
        if (fader.FadeCoroutine != null) fader.MonoBehaviour.StopCoroutine(fader.FadeCoroutine);
        fader.FadeCoroutine = fader.MonoBehaviour.StartCoroutine(Fade(fader.FadeTarget, alpha, duration, onDone));

        static IEnumerator Fade(CanvasGroup target, float alpha, float duration, Action onDone)
        {
            float time = 0;
            while (time < duration)
            {
                yield return null;
                if (time < duration) target.alpha += (alpha - target.alpha) * Time.deltaTime / (duration - time);
                time += Time.deltaTime;
            }
            target.alpha = alpha;
            onDone?.Invoke();
        }
    }
}

public interface IScaleAble<T> where T : MonoBehaviour
{
    T MonoBehaviour { get; }

    Coroutine ScaleCoroutine { get; set; }

    static void ScaleTo(IScaleAble<T> scaler, Vector3 scale, float duration, Action onDone = null)
    {
        if (scaler.ScaleCoroutine != null) scaler.MonoBehaviour.StopCoroutine(scaler.ScaleCoroutine);
        scaler.ScaleCoroutine = scaler.MonoBehaviour.StartCoroutine(Scale(scaler.MonoBehaviour.transform, scale, duration, onDone));

        static IEnumerator Scale(Transform target, Vector3 scale, float duration, Action onDone)
        {
            float time = 0;
            while (time < duration)
            {
                yield return null;
                if (time < duration) target.localScale += (scale - target.localScale) * Time.deltaTime / (duration - time);
                time += Time.deltaTime;
            }
            target.localScale = scale;
            onDone?.Invoke();
        }
    }
}

public interface IDragAble
{
    Sprite DragAbleIcon { get; }
}

public interface IManageAble
{
    public bool IsInit { get; }

    public bool Init();

    public bool Reset();

    public bool OnSaveGame(SaveData data);

    public bool OnLoadGame(SaveData data);
}

public interface IDirectionMove
{
    public void Move(Vector2 vector2);
}

public interface IBehaviourListener
{
    void OnStateEnter(AnimatorStateInfo stateInfo, int layerIndex);
    void OnStateUpdate(AnimatorStateInfo stateInfo, int layerIndex);
    void OnStateExit(AnimatorStateInfo stateInfo, int layerIndex);

}