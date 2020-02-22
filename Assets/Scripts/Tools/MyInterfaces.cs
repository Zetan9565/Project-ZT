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

public interface IDragAble
{
    Sprite DragAbleIcon { get; }
}