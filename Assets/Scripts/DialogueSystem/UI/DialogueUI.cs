using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogueUI : WindowUI
{
    public Text nameText;

    public Text wordsText;

    public Transform buttonArea;

    public Button backButton;
    public Button giftButton;
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

    protected override void Awake()
    {
        base.Awake();
        giftButton.onClick.AddListener(DialogueManager.Instance.SendTalkerGifts);
        warehouseButton.onClick.AddListener(DialogueManager.Instance.OpenTalkerWarehouse);
        shopButton.onClick.AddListener(DialogueManager.Instance.OpenTalkerShop);
        backButton.onClick.AddListener(DialogueManager.Instance.GoBackDefault);
        closeButton.onClick.AddListener(DialogueManager.Instance.CloseWindow);
        questButton.onClick.AddListener(DialogueManager.Instance.ShowTalkerQuest);
        pageUpButton.onClick.AddListener(DialogueManager.Instance.OptionPageUp);
        pageDownButton.onClick.AddListener(DialogueManager.Instance.OptionPageDown);
    }
}
