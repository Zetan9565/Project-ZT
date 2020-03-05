using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSeletionUI : WindowUI
{
    public Text windowTitle;
    public GameObject tips;

    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public ScrollRect gridScrollRect;
    public GameObject placementArea;

    public Button confirmButton;
    public Button clearButton;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(ItemSelectionManager.Instance.CloseWindow);
        confirmButton.onClick.AddListener(ItemSelectionManager.Instance.Confirm);
        clearButton.onClick.AddListener(ItemSelectionManager.Instance.Clear);
    }
}
