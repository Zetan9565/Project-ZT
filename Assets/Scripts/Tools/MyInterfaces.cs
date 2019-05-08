using UnityEngine;

public interface IWindow
{
    /// <summary>
    /// 在这个方法中把自己入栈到WindowsManager的窗口栈中
    /// </summary>
    void OpenWindow();

    void CloseWindow();

    void OpenCloseWindow();

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

    Canvas SortCanvas { get; }
}

public interface IDragable
{
    Sprite DragableIcon { get; }
}