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

public interface IOpenCloseAbleWindow
{
    void OpenCloseWindow();
}

public interface IFadeAble<T> where T : MonoBehaviour
{
    T MonoBehaviour { get; }

    Coroutine FadeCoroutine { get; set; }

    IEnumerator Fade(float alpha, float duration);
}

public interface IScaleAble<T> where T : MonoBehaviour
{
    T MonoBehaviour { get; }

    Vector3 OriginalScale { get; }

    Coroutine ScaleCoroutine { get; set; }

    IEnumerator Scale(Vector3 scale, float duration);
}

public interface IDragAble
{
    Sprite DragAbleIcon { get; }
}