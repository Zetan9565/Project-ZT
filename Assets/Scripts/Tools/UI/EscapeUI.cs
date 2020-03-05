using UnityEngine;
using UnityEngine.UI;

public class EscapeUI : WindowUI
{
    public Button exitButton;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(EscapeMenuManager.Instance.CloseWindow);
        exitButton.onClick.AddListener(EscapeMenuManager.Instance.Exit);
    }
}
