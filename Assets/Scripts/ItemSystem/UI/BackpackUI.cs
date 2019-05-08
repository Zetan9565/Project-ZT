using UnityEngine;
using UnityEngine.UI;

public class BackpackUI : MonoBehaviour
{
    public CanvasGroup backpackWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public Toggle[] tabs;

    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public Text money;
    public Text weight;
    public Text size;

    public Button closeButton;
    public Button sortButton;

    public GameObject discardArea;
    public ScrollRect gridRect;
    public Image gridMask;

    private void Awake()
    {
        if (!backpackWindow.GetComponent<GraphicRaycaster>()) backpackWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = backpackWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        closeButton.onClick.AddListener(BackpackManager.Instance.CloseWindow);
        sortButton.onClick.AddListener(BackpackManager.Instance.Sort);
        for (int i = 0; i < tabs.Length; i++)
        {
            int num = i;
            tabs[i].onValueChanged.AddListener(delegate { BackpackManager.Instance.SetPage(num); });
        }
        if (!discardArea.GetComponent<DiscardArea>()) discardArea.AddComponent<DiscardArea>();
    }

    private void OnDestroy()
    {
        if (BackpackManager.Instance) BackpackManager.Instance.ResetUI();
    }
}