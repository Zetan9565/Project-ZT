using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        MakeWindows(1);
        ShowWindows(itemSlot.MItemInfo, null, null, buttonDatas);
    }
    public void ShowItemInfo(ItemSlotBase itemSlot, ItemInfo contrast, params ButtonWithTextData[] buttonDatas)
    {
        this.itemSlot = itemSlot;
        MakeWindows(2);
        ShowWindows(itemSlot.MItemInfo, contrast, null, buttonDatas);
    }
    public void ShowItemInfo(ItemSlotBase itemSlot, ItemInfo contrast1, ItemInfo contrast2, params ButtonWithTextData[] buttonDatas)
    {
        this.itemSlot = itemSlot;
        MakeWindows(3);
        ShowWindows(itemSlot.MItemInfo, contrast1, contrast2, buttonDatas);
    }

    private void MakeWindows(int count)
    {
        if (windows.Count < count)
        {
            if (windows.Count < 1)
            {
                ItemInfoDisplayer window;
                if (ZetanUtility.IsPrefab(UI.windowPrefab.gameObject))
                    window = ObjectPool.Get(UI.windowPrefab, UI.windowParent);
                else window = UI.windowPrefab;
                windows.Add(window);
            }
            else
                while (windows.Count < count)
                {
                    ItemInfoDisplayer window = ObjectPool.Get(UI.windowPrefab, UI.windowParent);
                    window.Clear();
                    windows.Add(ObjectPool.Get(UI.windowPrefab, UI.windowParent));
                }
        }
    }
    private void ShowWindows(ItemInfo info, ItemInfo contrast1, ItemInfo contrast2, params ButtonWithTextData[] buttonDatas)
    {
        if (windows.Count > 0)
            if (info) windows[0].ShowItemInfo(info);
            else windows[0].Hide(true);
        if (windows.Count > 1)
            if (contrast1) windows[1].ShowItemInfo(contrast1);
            else windows[1].Hide(true);
        if (windows.Count > 2)
            if (contrast2) windows[2].ShowItemInfo(contrast2);
            else windows[2].Hide(true);

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
            UI.buttonArea.transform.SetAsLastSibling();
            UI.window.transform.position = new Vector2(position.x - rectAgent.width * 0.5f - (winWidth + rectButton.width) * 0.5f, UI.window.transform.position.y);
        }
        else
        {
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