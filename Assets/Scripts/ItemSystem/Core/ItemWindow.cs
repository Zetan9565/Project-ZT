using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemWindow : Window
{
    public ItemInfoDisplayer windowPrefab;
    public Transform windowParent;

    public RectTransform buttonArea;
    public Transform buttonParent;
    public ButtonWithText buttonPrefab;

    private ItemSlotBase itemSlot;
    public ItemData Data { get; private set; }

    private readonly List<ItemInfoDisplayer> windows = new List<ItemInfoDisplayer>();
    private readonly List<ButtonWithText> buttons = new List<ButtonWithText>();
    private readonly Stack<ButtonWithText> buttonsCache = new Stack<ButtonWithText>();

    private Coroutine keepCoroutine;

    private void MakeWindows(int count)
    {
        if (windows.Count < count)
        {
            if (windows.Count < 1)
                InitWindow(ZetanUtility.IsPrefab(windowPrefab.gameObject) ? ObjectPool.Get(windowPrefab, windowParent) : windowPrefab);
            else
                while (windows.Count < count)
                {
                    InitWindow(ObjectPool.Get(windowPrefab, windowParent));
                }
        }

        void InitWindow(ItemInfoDisplayer window)
        {
            window.Clear();
            window.Hide();
            windows.Add(window);
        }
    }
    private void InitWindows(ItemData data, ItemData[] contrast = null, params ButtonWithTextData[] buttonDatas)
    {
        if (contrast != null)
        {
            contrast = contrast.Where(x => x && x.Model).ToArray();
            MakeWindows(contrast.Length + 1);
        }
        else MakeWindows(1);
        foreach (var window in windows)
        {
            window.Hide(true);
        }

        if (data)
        {
            windows[0].ShowItemInfo(data);
        }
        if (contrast != null)
            for (int i = 0; i < contrast.Length; i++)
            {
                windows[i + 1].ShowItemInfo(contrast[i], true);
            }

        LeftOrRight(itemSlot.transform.position);

#if UNITY_ANDROID
        ZetanUtility.SetActive(buttonArea, true);
        ClearButtons();
        if (buttonDatas != null && buttonDatas.Length > 0)
        {
            ZetanUtility.SetActive(buttonParent, true);
            foreach (var bd in buttonDatas)
            {
                MakeButton(bd);
            }
        }
        else ZetanUtility.SetActive(buttonParent, false);
#elif UNITY_STANDALONE
        ClearButtons();
        ZetanUtility.SetActive(buttonArea, false);
#endif
    }

    private void MakeButton(ButtonWithTextData buttonData)
    {
        ButtonWithText button;
        if (buttonsCache.Count > 0)
            button = buttonsCache.Pop();
        else
        {
            if (buttons.Count < 1 && !ZetanUtility.IsPrefab(buttonPrefab.gameObject))
                button = buttonPrefab;
            else button = ObjectPool.Get(buttonPrefab, buttonParent);
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
                Rect rectWin = ZetanUtility.GetScreenSpaceRect(windowPrefab.GetComponent<RectTransform>());
                winWidth += rectWin.width;
            }
        }
#if UNITY_ANDROID
        Rect rectButton = ZetanUtility.GetScreenSpaceRect(buttonArea.GetComponent<RectTransform>());
#elif UNITY_STANDALONE
        Rect rectButton = Rect.zero;
#endif
        if (Screen.width * 0.5f < position.x)//在屏幕右半边
        {
            foreach (var window in windows)
            {
                window.transform.SetAsFirstSibling();
            }
            buttonArea.transform.SetAsLastSibling();
            content.transform.position = new Vector2(position.x - rectAgent.width * 0.5f - (winWidth + rectButton.width) * 0.5f, content.transform.position.y);
        }
        else
        {
            foreach (var window in windows)
            {
                window.transform.SetAsLastSibling();
            }
            buttonArea.transform.SetAsFirstSibling();
            content.transform.position = new Vector2(position.x + rectAgent.width * 0.5f + (winWidth + rectButton.width) * 0.5f, content.transform.position.y);
        }
        if (keepCoroutine != null) StopCoroutine(keepCoroutine);
        keepCoroutine = StartCoroutine(KeepInScreen());
    }

    private IEnumerator KeepInScreen()
    {
        yield return new WaitForEndOfFrame();
        ZetanUtility.KeepInsideScreen(content.GetComponent<RectTransform>(), bottom: false);
    }

    protected override bool OnOpen(params object[] args)
    {
        if (args.Length > 0 && args[0] is ItemSlotBase slot)
        {
            itemSlot = slot;
            Data = slot.Item;
            InitWindows(slot.Item, ItemUtility.GetContrast(slot.Item), args.Length > 1 ? args[1] as ButtonWithTextData[] : null);
            return true;
        }
        return false;
    }

    protected override bool OnClose(params object[] args)
    {
        itemSlot = null;
        Data = null;
        foreach (var window in windows)
        {
            window.Hide(true);
        }
        return true;
    }
}