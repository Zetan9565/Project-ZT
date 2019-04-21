using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class BackpackUI : MonoBehaviour
{
    public CanvasGroup backpackWindow;

    //public ToggleGroup tabsGroup;
    public Toggle[] tabs;

    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public Text money;
    public Text weight;
    public Text size;

    public Button openButton;
    public Button closeButton;
    public Button sortButton;

    public GameObject discardArea;
    public ScrollRect gridRect;

    private void Awake()
    {
        openButton.onClick.AddListener(BackpackManager.Instance.OpenUI);
        closeButton.onClick.AddListener(BackpackManager.Instance.CloseUI);
        sortButton.onClick.AddListener(BackpackManager.Instance.Sort);
        for (int i = 0; i < tabs.Length; i++)
        {
            int num = i;
            tabs[i].onValueChanged.AddListener(delegate { BackpackManager.Instance.SetPage(num); });
        }
    }
}