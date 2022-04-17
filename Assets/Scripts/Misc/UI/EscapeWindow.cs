using UnityEngine;
using UnityEngine.UI;

public class EscapeWindow : Window
{
    [SerializeField]
    private Button exitButton;

    protected override void OnAwake()
    {
        exitButton.onClick.AddListener(Exit);
    }

    protected override bool OnOpen(params object[] args)
    {
        NewWindowsManager.HideAllExcept(true, this);
        return true;
    }
    protected override bool OnClose(params object[] args)
    {
        NewWindowsManager.HideAllExcept(false);
        return true;
    }

    private void Exit()
    {
        ConfirmWindow.StartConfirm("确定退出" + Application.productName + "吗？", Application.Quit);
    }
}
