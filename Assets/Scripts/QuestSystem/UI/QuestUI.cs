using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class QuestUI : MonoBehaviour
{
    public CanvasGroup questsWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public Button closeWindow;

    public GameObject questPrefab;

    public GameObject questGroupPrefab;

    public Transform questListParent;
    public Transform cmpltQuestListParent;

    public CanvasGroup descriptionWindow;

    public Text descriptionText;

    public Button abandonButton;
    public Button traceButton;

    public Button closeDescription;

    public Text moneyText;

    public Text EXPText;

    public GameObject rewardCellPrefab;
    public Transform rewardCellsParent;

    public List<ItemAgent> rewardCells = new List<ItemAgent>();

    public CanvasGroup questBoard;

    public GameObject boardQuestPrefab;

    public Transform questBoardArea;

    private void Awake()
    {
        if (!questsWindow.gameObject.GetComponent<GraphicRaycaster>()) questsWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = questsWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        closeWindow.onClick.AddListener(QuestManager.Instance.CloseWindow);
        abandonButton.onClick.AddListener(QuestManager.Instance.AbandonSelectedQuest);
        traceButton.onClick.AddListener(QuestManager.Instance.TraceSelectedQuest);
        closeDescription.onClick.AddListener(QuestManager.Instance.HideDescription);
        foreach (ItemAgent rwc in rewardCells)
        {
            if (rwc) rwc.Clear(true);
        }
        rewardCells.Clear();
        for (int i = 0; i < 10; i++)
        {
            ItemAgent rwc = ObjectPool.Instance.Get(rewardCellPrefab, rewardCellsParent).GetComponent<ItemAgent>();
            rwc.Clear();
            rwc.Init();
            rewardCells.Add(rwc);
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance) QuestManager.Instance.ResetUI();
    }
}
