using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MakingUI : MonoBehaviour
{
    public CanvasGroup makingWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public Toggle[] tabs;

    public GameObject itemCellPrefab;
    public Transform itemCellsParent;

    public Button closeButton;

    public CanvasGroup descriptionWindow;

    public Text nameText;

    public ItemAgent icon;

    public Button makeButton;

    public Text description;

    private void Awake()
    {
        if (!makingWindow.GetComponent<GraphicRaycaster>()) makingWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = makingWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        icon.Init(ItemAgentType.Making);
        closeButton.onClick.AddListener(MakingManager.Instance.CloseWindow);
        makeButton.onClick.AddListener(MakingManager.Instance.MakeCurrent);
        if (tabs != null)
            for (int i = 0; i < tabs.Length; i++)
            {
                int num = i;
                tabs[i].onValueChanged.AddListener(delegate
                {
                    if (MakingManager.Instance) MakingManager.Instance.SetPage(num);
                });
            }
        if (tabs != null && tabs.Length > 0)
            tabs[0].isOn = true;
    }

    private void OnDestroy()
    {
        if (MakingManager.Instance) MakingManager.Instance.ResetUI();
    }
}
