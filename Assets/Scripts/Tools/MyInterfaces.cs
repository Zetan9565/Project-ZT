using UnityEngine;

public interface IOpenCloseable
{
    /// <summary>
    /// 必须在这个方法中把自己入栈到WindowsManager的窗口栈中
    /// </summary>
    void OpenUI();

    void CloseUI();

    void OpenCloseUI();

    void PauseDisplay(bool state);

    bool IsUIOpen { get; }

    bool IsPausing { get; }
}

public interface IDragable
{
    Sprite DragableIcon { get; }
}