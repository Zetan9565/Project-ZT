using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class QuestUI : MonoBehaviour
{
    [Header("任务窗口相关")]
    public CanvasGroup questsWindow;

    public Button openWindow;

    public Button closeWindow;

    public GameObject questPrefab;

    public GameObject questGroupPrefab;

    public Transform questListParent;
    public Transform cmpltQuestListParent;

    [Header("任务详情相关")]
    public CanvasGroup descriptionWindow;

    public Text descriptionText;

    public Button abandonButton;

    public Button closeDescription;

    public Text moneyText;

    public Text EXPText;

    public GameObject rewardCellPrefab;
    public Transform rewardCellsParent;

    public List<ItemAgent> rewardCells = new List<ItemAgent>();

    [Header("任务栏相关")]
    public GameObject boardQuestPrefab;

    public Transform questBoardArea;

    private void Awake()
    {
        openWindow.onClick.AddListener(QuestManager.Instance.OpenUI);
        closeWindow.onClick.AddListener(QuestManager.Instance.CloseUI);
        abandonButton.onClick.AddListener(QuestManager.Instance.AbandonSelectedQuest);
        closeDescription.onClick.AddListener(QuestManager.Instance.CloseDescriptionWindow);
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
