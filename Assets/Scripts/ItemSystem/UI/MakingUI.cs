using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MakingUI : WindowUI
{
    public Dropdown pageSelector;

    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public CanvasGroup descriptionWindow;

    public Text nameText;

    public ItemSlot icon;

    public Button makeButton;
    public Toggle loopToggle;

    public Button DIYButton;

    public Text description;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(MakingManager.Instance.CloseWindow);
        makeButton.onClick.AddListener(MakingManager.Instance.MakeCurrent);
        DIYButton.onClick.AddListener(MakingManager.Instance.DIY);
        pageSelector.onValueChanged.AddListener(MakingManager.Instance.SetPage);
    }
}
