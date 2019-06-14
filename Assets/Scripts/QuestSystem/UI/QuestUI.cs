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
    public Toggle questListToggle;
    public Transform cmpltQuestListParent;
    public Toggle cmpltQuestListToggle;

    public CanvasGroup descriptionWindow;

    public Text descriptionText;

    public Button abandonButton;
    public Button traceButton;

    public Button closeDescription;

    public Text moneyText;

    public Text EXPText;

    public GameObject rewardCellPrefab;
    public Transform rewardCellsParent;

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
        questListToggle.onValueChanged.AddListener(questListParent.gameObject.SetActive);
        questListToggle.group.RegisterToggle(questListToggle); ;
        cmpltQuestListToggle.onValueChanged.AddListener(cmpltQuestListParent.gameObject.SetActive);
        cmpltQuestListToggle.group.RegisterToggle(cmpltQuestListToggle);
        questListToggle.isOn = true;
        cmpltQuestListToggle.isOn = false;
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance) QuestManager.Instance.ResetUI();
    }
}
