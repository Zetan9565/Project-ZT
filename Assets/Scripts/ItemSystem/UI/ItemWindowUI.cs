using UnityEngine;
using UnityEngine.UI;

public class ItemWindowUI : ItemWindowBaseUI
{
    public Button closeButton;

    public Button mulFunButton;

    public Button discardButton;

    public GameObject buttonsArea;

    [HideInInspector]
    public CanvasGroup buttonAreaCanvas;

    private void Awake()
    {
        if (!itemWindow.GetComponent<GraphicRaycaster>()) itemWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = itemWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        windowsRect = itemWindow.GetComponent<RectTransform>();
        ZetanUtilities.SetActive(closeButton.gameObject, false);
#if UNITY_STANDALONE
        MyUtilities.SetActive(buttonsArea, false);
#elif UNITY_ANDROID
        if (!buttonsArea.GetComponent<CanvasGroup>()) buttonAreaCanvas = buttonsArea.AddComponent<CanvasGroup>();
        buttonAreaCanvas.ignoreParentGroups = true;
        discardButton.onClick.AddListener(ItemWindowManager.Instance.DiscardCurrentItem);
        closeButton.onClick.AddListener(ItemWindowManager.Instance.CloseItemWindow);
#endif
        gemstone_1.Clear();
        gemstone_2.Clear();
    }
}