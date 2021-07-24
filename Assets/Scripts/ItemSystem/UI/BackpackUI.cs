using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BackpackUI : WindowUI
{
    public Dropdown pageSelector;

    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public Text money;
    public Text weight;
    public Text size;

    public Button sortButton;
    public MakingTool handworkButton;

    public DiscardButton discardButton;
    public ScrollRect gridScrollRect;
    public Image gridMask;

    public InputField searchInput;
    public Button searchButton;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(BackpackManager.Instance.CloseWindow);
        sortButton.onClick.AddListener(BackpackManager.Instance.Arrange);
        pageSelector.onValueChanged.AddListener(BackpackManager.Instance.SetPage);
        if (!handworkButton.GetComponent<Button>()) handworkButton.gameObject.AddComponent<Button>();
        handworkButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            MakingManager.Instance.Make(handworkButton);
            MakingManager.Instance.OpenWindow();
        });
        searchButton.onClick.AddListener(BackpackManager.Instance.Search);
    }
}