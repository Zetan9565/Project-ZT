using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootUI : MonoBehaviour
{
    public CanvasGroup lootWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public Button takeAllButton;
    public Button closeButton;

    private void Awake()
    {
        if (!lootWindow.GetComponent<GraphicRaycaster>()) lootWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = lootWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        takeAllButton.onClick.AddListener(LootManager.Instance.TakeAll);
        closeButton.onClick.AddListener(LootManager.Instance.CloseWindow);
    }
}
