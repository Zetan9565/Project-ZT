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

    public ItemAgent icon;

    public Button makeButton;

    public Text description;

    protected override void Awake()
    {
        base.Awake();
        icon.Init(ItemAgentType.Making);
        closeButton.onClick.AddListener(MakingManager.Instance.CloseWindow);
        makeButton.onClick.AddListener(MakingManager.Instance.MakeCurrent);
        pageSelector.onValueChanged.AddListener(MakingManager.Instance.SetPage);
    }
}
