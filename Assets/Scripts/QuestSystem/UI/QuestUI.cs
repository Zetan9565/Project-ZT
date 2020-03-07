using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class QuestUI : WindowUI
{
    public GameObject questPrefab;

    public GameObject questGroupPrefab;

    public Transform questList;
    public Transform questListParent;
    public Toggle questListToggle;

    public Transform cmpltQuestList;
    public Transform cmpltQuestListParent;
    public Toggle cmpltQuestListToggle;

    public CanvasGroup descriptionWindow;

    public Text descriptionText;

    public Button abandonButton;
    public Button traceButton;

    public Button desCloseButton;

    public Text moneyText;

    public Text EXPText;

    public GameObject rewardCellPrefab;
    public Transform rewardCellsParent;

    public CanvasGroup questBoard;

    public GameObject boardQuestPrefab;

    public Transform questBoardArea;

    public Sprite questIcon;

    public QuestFlag questFlagsPrefab;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(QuestManager.Instance.CloseWindow);
        abandonButton.onClick.AddListener(QuestManager.Instance.AbandonSelectedQuest);
        traceButton.onClick.AddListener(QuestManager.Instance.TraceSelectedQuest);
        desCloseButton.onClick.AddListener(QuestManager.Instance.HideDescription);
        questListToggle.onValueChanged.AddListener(questList.gameObject.SetActive);
        questListToggle.group.RegisterToggle(questListToggle);
        cmpltQuestListToggle.onValueChanged.AddListener(cmpltQuestList.gameObject.SetActive);
        cmpltQuestListToggle.group.RegisterToggle(cmpltQuestListToggle);
        questListToggle.isOn = true;
        cmpltQuestListToggle.isOn = false;
    }
}
