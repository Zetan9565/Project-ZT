﻿using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class WindowButton : MonoBehaviour
{
    [TypeSelector(typeof(Window))]
    public string type;

    public bool openClose = true;


    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OpenClose);
    }

    private void OpenClose()
    {
        if (!openClose) WindowsManager.OpenWindow(ZetanUtility.GetTypeByFullName(type));
        else WindowsManager.OpenClose(ZetanUtility.GetTypeByFullName(type));
    }
}