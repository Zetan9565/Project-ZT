using UnityEngine;
using UnityEngine.UI;

public class LootUI : WindowUI
{
    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public Button takeAllButton;

    protected override void Awake()
    {
        base.Awake();
        takeAllButton.onClick.AddListener(LootManager.Instance.TakeAll);
        closeButton.onClick.AddListener(LootManager.Instance.CloseWindow);
    }
}
