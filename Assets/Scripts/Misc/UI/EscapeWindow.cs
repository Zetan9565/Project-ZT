﻿using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;
using ZetanStudio.UI;

public class EscapeWindow : Window
{
    [SerializeField]
    private Dropdown language;

    [SerializeField]
    private Button exitButton;

    private float scaleBef;

    protected override void OnAwake()
    {
        exitButton.onClick.AddListener(Exit);
        language.ClearOptions();
        language.AddOptions(Localization.Instance.LanguageNames.ToList());
        language.value = Language.LanguageIndex;
        language.onValueChanged.AddListener(SetLanguage);
    }

    private void SetLanguage(int lang)
    {
        Language.LanguageIndex = lang;
    }

    protected override bool OnOpen(params object[] args)
    {
        if (IsOpen) return false;
        WindowsManager.HideAll(true);
        scaleBef = Time.timeScale;
        Time.timeScale = 0;
        return true;
    }
    protected override bool OnClose(params object[] args)
    {
        WindowsManager.HideAll(false);
        Time.timeScale = scaleBef;
        return true;
    }

    private void Exit()
    {
        ConfirmWindow.StartConfirm("确定退出" + Application.productName + "吗？", Application.Quit);
    }
}
