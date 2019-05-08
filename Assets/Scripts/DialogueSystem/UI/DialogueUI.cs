using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogueUI : MonoBehaviour
{
    public CanvasGroup dialogueWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public Text nameText;

    public Text wordsText;

    public Button backButton;

    public Button finishButton;

    public Button warehouseButton;
    public Button shopButton;

    public GameObject optionPrefab;

    public Transform optionsParent;

    public Button pageUpButton;

    public Button pageDownButton;

    public Text pageText;


    public float textLineHeight = 22.35832f;

    public int lineAmount = 5;

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
        if (!dialogueWindow.gameObject.GetComponent<GraphicRaycaster>()) dialogueWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = dialogueWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        warehouseButton.onClick.AddListener(DialogueManager.Instance.OpenTalkerWarehouse);
        shopButton.onClick.AddListener(DialogueManager.Instance.OpenTalkerShop);
        backButton.onClick.AddListener(DialogueManager.Instance.GotoDefault);
        finishButton.onClick.AddListener(DialogueManager.Instance.CloseWindow);
        questButton.onClick.AddListener(DialogueManager.Instance.LoadTalkerQuest);
        pageUpButton.onClick.AddListener(DialogueManager.Instance.OptionPageUp);
        pageDownButton.onClick.AddListener(DialogueManager.Instance.OptionPageDown);
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

    public void OnDestroy()
    {
        if (DialogueManager.Instance) DialogueManager.Instance.ResetUI();
    }
}
