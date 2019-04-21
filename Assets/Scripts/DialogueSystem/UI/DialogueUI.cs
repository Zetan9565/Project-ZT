using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogueUI : MonoBehaviour
{
    [Header("对话框相关")]
    public CanvasGroup dialogueWindow;

    public Text nameText;

    public Text wordsText;

    public Button talkButton;

    public Button backButton;

    public Button finishButton;

    [Header("选项相关")]

    public GameObject optionPrefab;

    public Transform optionsParent;

    public Button pageUpButton;

    public Button pageDownButton;

    public Text pageText;


    public float textLineHeight = 22.35832f;

    public int lineAmount = 5;

    [Header("任务详情相关")]

    public Button questButton;

    public CanvasGroup descriptionWindow;

    public Text descriptionText;

    public Text moneyText;
    public Text EXPText;

    public GameObject rewardCellPrefab;
    public Transform rewardCellsParent;

    public List<ItemAgent> rewardCells = new List<ItemAgent>();

    private void Awake()
    {
        talkButton.onClick.AddListener(DialogueManager.Instance.OpenDialogueWindow);
        backButton.onClick.AddListener(DialogueManager.Instance.GotoDefault);
        finishButton.onClick.AddListener(DialogueManager.Instance.CloseDialogueWindow);
        questButton.onClick.AddListener(DialogueManager.Instance.LoadTalkerQuest);
        pageUpButton.onClick.AddListener(DialogueManager.Instance.OptionPageUp);
        pageDownButton.onClick.AddListener(DialogueManager.Instance.OptionPageDown);
        foreach (ItemAgent rwc in rewardCells)
        {
            if (rwc) rwc.Clear(false, true);
        }
        rewardCells.Clear();
        for (int i = 0; i < 10; i++)
        {
            ItemAgent rwc = ObjectPool.Instance.Get(rewardCellPrefab, rewardCellsParent).GetComponent<ItemAgent>();
            rwc.Clear();
            rewardCells.Add(rwc);
        }
    }
}
