using System;
using System.Collections;
using UnityEngine;

public interface IForEach<out T>
{
    public void ForEach(Action<T> action);
}
public interface IForEachBreakable<out T>
{
    /// <summary>
    /// 带中断的遍历
    /// </summary>
    /// <param name="action">返回值表示是否中断的访问器</param>
    public void ForEach(Predicate<T> action);
}

public interface IHideable
{
    bool IsHidden { get; }
    /// <summary>
    /// 显隐窗口，不应单独使用，显隐窗口请使用<see cref="WindowsManager.HideWindow(string, bool, object[])"/>
    /// 、<see cref="WindowsManager.HideWindow{T}(bool, object[])"/>、<see cref="WindowsManager.HideWindow(IHideable, bool, object[])"/>
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
                if (time < duration) target.alpha += (alpha - target.alpha) * Time.unscaledDeltaTime / (duration - time);
                time += Time.unscaledDeltaTime;
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
                if (time < duration) target.localScale += (scale - target.localScale) * Time.unscaledDeltaTime / (duration - time);
                time += Time.unscaledDeltaTime;
            }
            target.localScale = scale;
            onDone?.Invoke();
        }
    }
}

public interface IDraggable
{
    Sprite DraggableIcon { get; }
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