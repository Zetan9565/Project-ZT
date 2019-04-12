using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogueUI : MonoBehaviour
{
    [SerializeField, Header("对话框相关")]
    public CanvasGroup dialogueWindow;
    [SerializeField]
    public Text nameText;
    [SerializeField]
    public Text wordsText;
    [SerializeField]
    public Button talkButton;
    [SerializeField]
    public Button backButton;
    [SerializeField]
    public Button finishButton;

    [Header("选项相关")]
    [SerializeField]
    public GameObject optionPrefab;
    [SerializeField]
    public Transform optionsParent;
    [SerializeField]
    public Button pageUpButton;
    [SerializeField]
    public Button pageDownButton;
    [SerializeField]
    public Text pageText;

    [SerializeField]
    public float textLineHeight = 22.35832f;
    [SerializeField]
    public int lineAmount = 5;

    [Header("任务详情相关")]
    [SerializeField]
    public Button questButton;
    [SerializeField]
    public CanvasGroup descriptionWindow;
    [SerializeField]
    public Text descriptionText;
    [SerializeField]
    public Text money_EXPText;
    [SerializeField]
    public ItemAgent[] rewardCells;

    private void Awake()
    {
        talkButton.onClick.AddListener(DialogueManager.Instance.OpenDialogueWindow);
        backButton.onClick.AddListener(DialogueManager.Instance.GotoDefault);
        finishButton.onClick.AddListener(DialogueManager.Instance.CloseDialogueWindow);
        questButton.onClick.AddListener(DialogueManager.Instance.LoadTalkerQuest);
    }
}
