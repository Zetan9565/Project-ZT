using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/退出菜单管理器")]
public class EscapeMenuManager : WindowHandler<EscapeUI, EscapeMenuManager>, IOpenCloseAbleWindow
{
    public override void OpenWindow()
    {
        base.OpenWindow();
        if (!IsUIOpen) return;
        WindowsManager.Instance.PauseAll(true, this);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        WindowsManager.Instance.PauseAll(false);
    }

    public void OpenCloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) OpenWindow();
        else CloseWindow();
    }

    public override void SetUI(EscapeUI UI)
    {
        IsPausing = false;
        CloseWindow();
        base.SetUI(UI);
    }

    public void Exit()
    {
        ConfirmManager.Instance.New("确定退出" + Application.productName + "吗？", Application.Quit);
    }
}
