﻿using LeoLuz.PlugAndPlayJoystick;
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
    private AnalogicKnob joyStick;
    public AnalogicKnob JoyStick
    {
        get
        {
            return joyStick;
        }
    }

    [SerializeField]
    private UIButtonToButton interactiveButton;
    public UIButtonToButton InteractiveButton
    {
        get
        {
            return interactiveButton;
        }
    }

    [SerializeField]
    private Text interactiveName;

    private static bool dontDestroyOnLoadOnce;
    private void Awake()
    {
#if UNITY_STANDALONE
        EnableJoyStick(false);
        ZetanUtility.SetActive(JoyStick.gameObject, false);
        ZetanUtility.SetActive(JoyStick.KnobBackground.gameObject, false);
#elif UNITY_ANDROID
        ZetanUtility.SetActive(JoyStick.gameObject, true);
        ZetanUtility.SetActive(JoyStick.KnobBackground.gameObject, true);
#endif
        ZetanUtility.SetActive(InteractiveButton.gameObject, false);
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
        JoyStick.enabled = value && !(DialogueManager.Instance.IsUIOpen || ShopManager.Instance.IsUIOpen ||
            WarehouseManager.Instance.IsUIOpen || QuestManager.Instance.IsUIOpen || BuildingManager.Instance.IsPreviewing);
    }

    public void EnableInteractive(bool value, string name = null)
    {
#if UNITY_ANDROID
        if (!value)
            ZetanUtility.SetActive(InteractiveButton.gameObject, false);
        else
        {
            ZetanUtility.SetActive(InteractiveButton.gameObject, true &&
                ((DialogueManager.Instance.TalkAble &&
                !WarehouseManager.Instance.IsUIOpen &&
                !LootManager.Instance.IsUIOpen &&
                !GatherManager.Instance.IsGathering &&
                !PlantManager.Instance.IsUIOpen) ||//对话时无法激活
                (WarehouseManager.Instance.StoreAble &&
                !DialogueManager.Instance.IsUIOpen &&
                !LootManager.Instance.IsUIOpen &&
                !GatherManager.Instance.IsGathering &&
                !PlantManager.Instance.IsUIOpen) ||//使用仓库时无法激活
                (LootManager.Instance.PickAble &&
                !DialogueManager.Instance.IsUIOpen &&
                !WarehouseManager.Instance.IsUIOpen &&
                !GatherManager.Instance.IsGathering &&
                !PlantManager.Instance.IsUIOpen) ||//拾取时无法激活
                (GatherManager.Instance.GatherAble &&
                !DialogueManager.Instance.TalkAble &&
                !WarehouseManager.Instance.IsUIOpen &&
                !LootManager.Instance.IsUIOpen &&
                !PlantManager.Instance.IsUIOpen) ||//采集时无法激活
                ((FieldManager.Instance.ManageAble || PlantManager.Instance.PlantAble) &&
                !DialogueManager.Instance.TalkAble &&
                !WarehouseManager.Instance.IsUIOpen &&
                !LootManager.Instance.IsUIOpen &&
                !GatherManager.Instance.IsGathering)//种植时无法激活
                ));
        }
#endif
        if (!string.IsNullOrEmpty(name) && value)
        {
            ZetanUtility.SetActive(interactiveName.transform.parent.gameObject, true);
            interactiveName.text = name;
        }
        else
        {
            ZetanUtility.SetActive(interactiveName.transform.parent.gameObject, false);
            interactiveName.text = string.Empty;
        }
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
    }

    public void Init()
    {
        WindowsManager.Instance.CloseAll();

        DragableManager.Instance.ResetIcon();
        ProgressBar.Instance.CancelWithoutEvent();

        QuestManager.Instance.SetUI(FindObjectOfType<QuestUI>());
        BackpackManager.Instance.SetUI(FindObjectOfType<BackpackUI>());
        BackpackManager.Instance.Init();
        WarehouseManager.Instance.SetUI(FindObjectOfType<WarehouseUI>());
        DialogueManager.Instance.SetUI(FindObjectOfType<DialogueUI>());
        BuildingManager.Instance.SetUI(FindObjectOfType<BuildingUI>());
        ShopManager.Instance.SetUI(FindObjectOfType<ShopUI>());
        EscapeMenuManager.Instance.SetUI(FindObjectOfType<EscapeUI>());
        CalendarManager.Instance.SetUI(FindObjectOfType<CalendarUI>());
    }
}
