using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonoBehaviour<UIManager>
{
    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private Button questButton;

    [SerializeField]
    private Button backpackButton;

    [SerializeField]
    private Button calendarButton;

    [SerializeField]
    private Button buildingButton;

    [SerializeField]
    private Button settingButton;

    [SerializeField]
    private Transform questFlagParent;
    [SerializeField]
    private Transform buildingFlagParent;

    [SerializeField]
    private Joystick joyStick;
    public Joystick JoyStick => joyStick;

    public Transform QuestFlagParent
    {
        get
        {
            return questFlagParent ? questFlagParent : transform;
        }
    }

    public Transform BuildingFlagParent
    {
        get
        {
            return buildingFlagParent ? buildingFlagParent : transform;
        }
    }

    private static bool dontDestroyOnLoadOnce;
    private void Awake()
    {
#if UNITY_STANDALONE
        EnableJoyStick(false);
        ZetanUtility.SetActive(JoyStick.gameObject, false);
        ZetanUtility.SetActive(JoyStick.KnobBackground.gameObject, false);
#elif UNITY_ANDROID
        ZetanUtility.SetActive(JoyStick.gameObject, true);
#endif
        questButton.onClick.AddListener(QuestManager.Instance.OpenCloseWindow);
        backpackButton.onClick.AddListener(BackpackManager.Instance.OpenCloseWindow);
        calendarButton.onClick.AddListener(CalendarManager.Instance.OpenCloseWindow);
        buildingButton.onClick.AddListener(BuildingManager.Instance.OpenCloseWindow);
        settingButton.onClick.AddListener(EscapeMenuManager.Instance.OpenCloseWindow);
        if (!dontDestroyOnLoadOnce)
        {
            DontDestroyOnLoad(this);
            dontDestroyOnLoadOnce = true;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void EnableJoyStick(bool value)
    {
        if (Application.platform != RuntimePlatform.Android) joyStick.enabled = false;
        else
            JoyStick.enabled = value && !(DialogueManager.Instance.IsUIOpen || ShopManager.Instance.IsUIOpen ||
                WarehouseManager.Instance.IsUIOpen || QuestManager.Instance.IsUIOpen || BuildingManager.Instance.IsPreviewing);
        if (!JoyStick.enabled) JoyStick.Stop();
    }

    public void ShowAll()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        EnableJoyStick(true);
    }

    public void HideAll()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        JoyStick.enabled = false;
        JoyStick.Stop();
    }

    public void Init()
    {
        WindowsManager.Instance.CloseAll();

        DragableManager.Instance.ResetIcon();
        ProgressBar.Instance.Cancel();

        BackpackManager.Instance.SetUI(FindObjectOfType<BackpackUI>());
        BackpackManager.Instance.Init();
        BuildingManager.Instance.SetUI(FindObjectOfType<BuildingUI>());
        CalendarManager.Instance.SetUI(FindObjectOfType<CalendarUI>());
        DialogueManager.Instance.SetUI(FindObjectOfType<DialogueUI>());
        EscapeMenuManager.Instance.SetUI(FindObjectOfType<EscapeUI>());
        FieldManager.Instance.SetUI(FindObjectOfType<FieldUI>());
        InteractionManager.Instance.SetUI(FindObjectOfType<InteractionUI>());
        ItemSelectionManager.Instance.SetUI(FindObjectOfType<ItemSeletionUI>());
        ItemWindowManager.Instance.SetUI(FindObjectOfType<ItemWindowUI>());
        LootManager.Instance.SetUI(FindObjectOfType<LootUI>());
        MakingManager.Instance.SetUI(FindObjectOfType<MakingUI>());
        PlantManager.Instance.SetUI(FindObjectOfType<PlantUI>());
        QuestManager.Instance.SetUI(FindObjectOfType<QuestUI>());
        ShopManager.Instance.SetUI(FindObjectOfType<ShopUI>());
        WarehouseManager.Instance.SetUI(FindObjectOfType<WarehouseUI>());
    }
}
