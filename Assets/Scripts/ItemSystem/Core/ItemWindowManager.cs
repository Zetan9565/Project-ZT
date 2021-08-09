using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemWindowManager : WindowHandler<ItemWindowUI, ItemWindowManager>
{
    private ItemSlotBase itemSlot;
    public ItemInfo Info { get; private set; }

    private readonly List<ItemInfoDisplayer> windows = new List<ItemInfoDisplayer>();
    private readonly List<ButtonWithText> buttons = new List<ButtonWithText>();
    private readonly Stack<ButtonWithText> buttonsCache = new Stack<ButtonWithText>();

    private Coroutine keepCoroutine;

    public void ShowItemInfo(ItemSlotBase itemSlot, params ButtonWithTextData[] buttonDatas)
    {
        this.itemSlot = itemSlot;
        base.CloseWindow();
        ShowWindows(itemSlot.MItemInfo, BackpackManager.Instance.GetContrast(itemSlot.MItemInfo), buttonDatas);
    }

    private void MakeWindows(int count)
    {
        if (windows.Count < count)
        {
            if (windows.Count < 1)
                InitWindow(ZetanUtility.IsPrefab(UI.windowPrefab.gameObject) ? ObjectPool.Get(UI.windowPrefab, UI.windowParent) : UI.windowPrefab);
            else
                while (windows.Count < count)
                {
                    InitWindow(ObjectPool.Get(UI.windowPrefab, UI.windowParent));
                }
        }

        void InitWindow(ItemInfoDisplayer window)
        {
            window.Clear();
            window.Hide();
            windows.Add(window);
        }
    }
    private void ShowWindows(ItemInfo info, ItemInfo[] contrast = null, params ButtonWithTextData[] buttonDatas)
    {
        if (contrast != null)
        {
            contrast = contrast.Where(x => x && x.item).ToArray();
            MakeWindows(contrast.Length + 1);
        }
        else MakeWindows(1);
        foreach (var window in windows)
        {
            window.Hide(true);
        }

        if (info)
        {
            windows[0].ShowItemInfo(info);
        }
        if (contrast != null)
            for (int i = 0; i < contrast.Length; i++)
            {
                windows[i + 1].ShowItemInfo(contrast[i], true);
            }

        LeftOrRight(itemSlot.transform.position);

#if UNITY_ANDROID
        ZetanUtility.SetActive(UI.buttonArea, true);
        ClearButtons();
        if (buttonDatas != null)
        {
            ZetanUtility.SetActive(UI.buttonParent, true);
            foreach (var data in buttonDatas)
            {
                MakeButton(data);
            }
        }
        else ZetanUtility.SetActive(UI.buttonParent, false);
#elif UNITY_STANDALONE
        ZetanUtility.SetActive(UI.buttonArea, false);
#endif
        OpenWindow();
    }
    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        itemSlot = null;
        Info = null;
        foreach (var window in windows)
        {
            window.Hide(true);
        }
    }

    private void MakeButton(ButtonWithTextData buttonData)
    {
        ButtonWithText button;
        if (buttonsCache.Count > 0)
            button = buttonsCache.Pop();
        else
        {
            if (buttons.Count < 1 && !ZetanUtility.IsPrefab(UI.buttonPrefab.gameObject))
                button = UI.buttonPrefab;
            else button = ObjectPool.Get(UI.buttonPrefab, UI.buttonParent);
        }
        button.transform.SetAsLastSibling();
        ZetanUtility.SetActive(button, true);
        button.Init(buttonData);
        buttons.Add(button);
    }
    private void ClearButtons()
    {
        foreach (var button in buttons)
        {
            ZetanUtility.SetActive(button, false);
            buttonsCache.Push(button);
        }
        buttons.Clear();
    }

    private void LeftOrRight(Vector2 position)
    {
        Rect rectAgent = ZetanUtility.GetScreenSpaceRect(itemSlot.GetComponent<RectTransform>());
        float winWidth = 0;
        for (int i = 0; i < windows.Count; i++)
        {
            var window = windows[i];
            if (window.gameObject.activeSelf)
            {
                Rect rectWin = ZetanUtility.GetScreenSpaceRect(UI.windowPrefab.GetComponent<RectTransform>());
                winWidth += rectWin.width;
            }
        }
#if UNITY_ANDROID
        Rect rectButton = ZetanUtility.GetScreenSpaceRect(UI.buttonArea.GetComponent<RectTransform>());
#elif UNITY_STANDALONE
        Rect rectButton = Rect.zero;
#endif
        if (Screen.width * 0.5f < position.x)//在屏幕右半边
        {
            foreach (var window in windows)
            {
                window.transform.SetAsFirstSibling();
            }
            UI.buttonArea.transform.SetAsLastSibling();
            UI.window.transform.position = new Vector2(position.x - rectAgent.width * 0.5f - (winWidth + rectButton.width) * 0.5f, UI.window.transform.position.y);
        }
        else
        {
            foreach (var window in windows)
            {
                window.transform.SetAsLastSibling();
            }
            UI.buttonArea.transform.SetAsFirstSibling();
            UI.window.transform.position = new Vector2(position.x + rectAgent.width * 0.5f + (winWidth + rectButton.width) * 0.5f, UI.window.transform.position.y);
        }
        if (keepCoroutine != null) StopCoroutine(keepCoroutine);
        keepCoroutine = StartCoroutine(KeepInScreen());
    }

    private IEnumerator KeepInScreen()
    {
        yield return null;
        ZetanUtility.KeepInsideScreen(UI.window.GetComponent<RectTransform>(), true, true, true, false);
    }

    public override void SetUI(ItemWindowUI UI)
    {
        foreach (var button in buttons)
        {
            if (button && button.gameObject) button.Recycle();
        }
        buttons.Clear();
        foreach (var button in buttonsCache)
        {
            if (button && button.gameObject) button.Recycle();
        }
        buttonsCache.Clear();
        this.UI = UI;
        //this.UI.subUI = UI.subUI;
#if UNITY_STANDALONE
        ZetanUtility.SetActive(UI.buttonArea, false);
#elif UNITY_ANDROID
        ZetanUtility.SetActive(UI.buttonArea, true);
#endif
    }
}