using UnityEngine;
using UnityEngine.UI;

public class PlantUI : WindowUI
{
    public Dropdown pageSelector;

    public GameObject seedCellPrefab;
    public Transform seedCellsParent;

    public CanvasGroup descriptionWindow;
    public Text nameText;
    public ItemSlot icon;
    public Text amount;
    public Text description;

    public InputField searchInput;
    public Button searchButton;

    public GameObject cancelArea;

    protected override void Awake()
    {
        base.Awake();
        icon.Init();
        closeButton.onClick.AddListener(PlantManager.Instance.CloseWindow);
        pageSelector.onValueChanged.AddListener(PlantManager.Instance.SetPage);
    }
}
