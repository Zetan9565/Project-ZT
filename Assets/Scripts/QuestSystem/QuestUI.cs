using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class QuestUI : MonoBehaviour
{
    [SerializeField, Header("任务窗口相关")]
    public CanvasGroup questsWindow;

    [SerializeField]
    public Button openWindow;

    [SerializeField]
    public Button closeWindow;

    [SerializeField]
    public GameObject questPrefab;

    [SerializeField]
    public GameObject questGroupPrefab;

    [SerializeField]
    public Transform questListParent;
    [SerializeField]
    public Transform cmpltQuestListParent;

    [Header("任务详情相关")]
    [SerializeField]
    public CanvasGroup descriptionWindow;

    [SerializeField]
    public Text descriptionText;

    [SerializeField]
    public Button abandonButton;

    [SerializeField]
    public Button closeDescription;

    [SerializeField]
    public Text money_EXPText;

    [SerializeField]
    public ItemAgent[] rewardCells;

    [SerializeField, Header("任务栏相关")]
    public GameObject boardQuestPrefab;

    [SerializeField]
    public Transform questBoardArea;

    private void Awake()
    {
        openWindow.onClick.AddListener(QuestManager.Instance.OpenQuestWindow);
        closeWindow.onClick.AddListener(QuestManager.Instance.CloseQuestWindow);
        abandonButton.onClick.AddListener(QuestManager.Instance.AbandonSelectedQuest);
        closeDescription.onClick.AddListener(QuestManager.Instance.CloseDescriptionWindow);
    }
}
