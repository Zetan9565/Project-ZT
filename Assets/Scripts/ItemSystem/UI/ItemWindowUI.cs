using UnityEngine;

public class ItemWindowUI : WindowUI
{
    public ItemInfoDisplayer windowPrefab;
    public Transform windowParent;

    public RectTransform buttonArea;
    public Transform buttonParent;
    public ButtonWithText buttonPrefab;

    public Transform cacheParent;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(ItemWindowManager.Instance.CloseWindow);
    }
}