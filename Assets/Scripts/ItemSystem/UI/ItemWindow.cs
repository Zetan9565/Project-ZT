using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.UI;
using ZetanStudio.InventorySystem;
#if UNITY_STANDALONE
using UnityEngine.UI;
#endif

namespace ZetanStudio.ItemSystem.UI
{
    [DisallowMultipleComponent]
    public class ItemWindow : Window
    {
        public ItemInfoDisplayer displayerPrefab;
        private ItemInfoDisplayer realPrefab;
        public Transform displayerParent;

        public GameObject buttonArea;
        public ButtonWithTextList buttonList;

        private ItemSlot itemSlot;
        private Vector2? position;
        public ItemData Item { get; private set; }

        private readonly List<ItemInfoDisplayer> displayers = new List<ItemInfoDisplayer>();

        private Coroutine keepCoroutine;
#if UNITY_STANDALONE
    private Coroutine hangCoroutine;
    private bool hangOn;
    private Canvas blocker;
#endif

        protected override void OnAwake()
        {
            realPrefab = Utility.IsPrefab(displayerPrefab.gameObject) ? displayerPrefab : Instantiate(displayerPrefab);
            Utility.SetActive(realPrefab, false);
        }

        protected override void RegisterNotify()
        {
            NotifyCenter.AddListener(BackpackManager.Instance.ItemAmountChangedMsgKey, OnItemAmountChanged, this);
        }

        private void OnItemAmountChanged(params object[] msg)
        {
            if (IsOpen && msg.Length > 2 && msg[0] == Item && (int)msg[1] < 1)
                Close();
        }

        private void MakeDisplayers(int amount)
        {
            if (displayers.Count < amount)
            {
                if (displayers.Count < 1)
                    InitDisplayer(Utility.IsPrefab(displayerPrefab.gameObject) ? ObjectPool.Get(displayerPrefab, displayerParent) : displayerPrefab);
                else
                    while (displayers.Count < amount)
                    {
                        InitDisplayer(ObjectPool.Get(realPrefab, displayerParent));
                    }
            }

            void InitDisplayer(ItemInfoDisplayer displayer)
            {
                displayer.SetWindow(this);
                displayer.Hide();
                displayers.Add(displayer);
            }
        }
        private void Refresh(ItemData data, params ButtonWithTextData[] buttonDatas)
        {
            MakeDisplayers(1);
            foreach (var window in displayers)
            {
                window.Hide();
            }

            if (data) displayers[0].ShowItem(data);
            if (itemSlot) LeftOrRight(itemSlot.transform.position);
            else LeftOrRight(position ?? Utility.ScreenCenter);

#if UNITY_ANDROID
            Utility.SetActive(buttonArea, true);
            buttonList.Refresh(buttonDatas);
#elif UNITY_STANDALONE
        Utility.SetActive(buttonArea, false);
        if (hangCoroutine != null) StopCoroutine(hangCoroutine);
        hangCoroutine = StartCoroutine(checkHangOn());

        IEnumerator checkHangOn()
        {
            while (true)
            {
                bool key = InputManager.GetKey(UnityEngine.InputSystem.Key.LeftAlt);
                if (key && !this.hangOn) hangOn();
                else if (!key && this.hangOn)
                {
                    hangUp();
                    yield break;
                }
                yield return null;
            }
        }

        void hangOn()
        {
            this.hangOn = true;
            if (!blocker)
            {
                blocker = new GameObject("Blocker", typeof(RectTransform), typeof(EmptyGraphic), typeof(GraphicRaycaster)).GetComponent<Canvas>();
                var rectTr = blocker.GetComponent<RectTransform>();
                rectTr.SetParent(transform, false);
                rectTr.anchorMin = Vector2.zero;
                rectTr.anchorMax = Vector2.one;
                rectTr.localScale = Vector3.one;
                rectTr.sizeDelta = Vector2.zero;
                rectTr.anchoredPosition = Vector2.zero;
                blocker.overrideSorting = true;
                blocker.sortingLayerID = SortingLayer.NameToID("UI");
                rectTr.SetAsFirstSibling();
            }
            blocker.sortingOrder = WindowCanvas.sortingOrder;
            Utility.SetActive(blocker, true);
        }
        void hangUp()
        {
            this.hangOn = false;
            Utility.SetActive(blocker, false);
            Close();
        }
#endif
        }

        public void SetContrast(params ItemData[] contrast)
        {
            if (contrast != null && contrast.Count(x => x && x.Model) > 0)
            {
                contrast = contrast.Where(x => x && x.Model).ToArray();
                MakeDisplayers(contrast.Length + 1);
                for (int i = 0; i < contrast.Length; i++)
                {
                    displayers[i + 1].ShowItem(contrast[i]);
                }
            }
            else
                for (int i = 1; i > 0 && i < displayers.Count; i++)
                {
                    displayers[i].Hide();
                    displayers.RemoveAt(i);
                    i--;
                }
        }

        private void LeftOrRight(Vector2 position)
        {
            Rect slotRect = itemSlot ? Utility.GetScreenSpaceRect(itemSlot.GetComponent<RectTransform>()) : Rect.zero;
            float winWidth = 0;
            for (int i = 0; i < displayers.Count; i++)
            {
                var window = displayers[i];
                if (window.gameObject.activeSelf)
                {
                    Rect rectWin = Utility.GetScreenSpaceRect(displayerPrefab.GetComponent<RectTransform>());
                    winWidth += rectWin.width;
                }
            }
#if UNITY_ANDROID
            Rect buttonRect = Utility.GetScreenSpaceRect(buttonArea.GetComponent<RectTransform>());
#elif UNITY_STANDALONE
        Rect buttonRect = Rect.zero;
#endif
            if (Screen.width * 0.5f <= position.x)//在屏幕右半边
            {
                foreach (var window in displayers)
                {
                    window.transform.SetAsFirstSibling();
                }
                buttonArea.transform.SetAsLastSibling();
                content.transform.position = new Vector2(position.x - slotRect.width * 0.5f - (winWidth + buttonRect.width) * 0.5f, content.transform.position.y);
            }
            else
            {
                foreach (var window in displayers)
                {
                    window.transform.SetAsLastSibling();
                }
                buttonArea.transform.SetAsFirstSibling();
                content.transform.position = new Vector2(position.x + slotRect.width * 0.5f + (winWidth + buttonRect.width) * 0.5f, content.transform.position.y);
            }
            if (keepCoroutine != null) StopCoroutine(keepCoroutine);
            keepCoroutine = StartCoroutine(KeepInScreen());
        }

        private IEnumerator KeepInScreen()
        {
            yield return new WaitForEndOfFrame();
            Utility.KeepInsideScreen(content.GetComponent<RectTransform>(), bottom: false);
        }

        protected override bool OnOpen(params object[] args)
        {
#if UNITY_STANDALONE
        if (hangOn) return false;
#endif
            if (args.Length > 0)
                if (args[0] is ItemSlot slot)
                {
                    itemSlot = slot;
                    position = null;
                    Item = slot.Item;
                    Refresh(Item, args.Length > 1 ? args[1] as ButtonWithTextData[] : null);
                    return true;
                }
                else if (args[0] is ItemData data)
                {
                    itemSlot = null;
                    position = args.Length > 1 && args[1] is Vector2 pos ? pos : null;
                    Item = data;
                    Refresh(Item, args.Length > 1 ? args[1] as ButtonWithTextData[] : null);
                    return true;
                }
                else if (args[0] is Item item)
                {
                    itemSlot = null;
                    position = args.Length > 1 && args[1] is Vector2 pos ? pos : null;
                    Item = ItemData.Empty(item);
                    Refresh(Item, args.Length > 1 ? args[1] as ButtonWithTextData[] : null);
                    return true;
                }
            return false;
        }

        protected override bool OnClose(params object[] args)
        {
#if UNITY_STANDALONE
        if (hangOn) return false;
        if (hangCoroutine != null) StopCoroutine(hangCoroutine);
#endif
            itemSlot = null;
            position = null;
            Item = null;
            return true;
        }

        public override void OnCloseComplete()
        {
            foreach (var window in displayers)
            {
                window.Hide();
            }
        }
    }
}