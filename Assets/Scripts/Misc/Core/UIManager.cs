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
#elif UNITY_ANDROID
        ZetanUtility.SetActive(JoyStick.gameObject, true);
#endif
        //questButton.onClick.AddListener(QuestManager.Instance.OpenCloseWindow);
        //backpackButton.onClick.AddListener(BackpackManager.Instance.OpenCloseWindow);
        //calendarButton.onClick.AddListener(CalendarManager.Instance.OpenCloseWindow);
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
#if UNITY_STANDALONE
        joyStick.enabled = false;
#elif UNITY_ANDROID
        JoyStick.enabled = value && !(NewWindowsManager.IsWindowOpen<DialogueWindow>() || NewWindowsManager.IsWindowOpen<ShopWindow>() ||
            NewWindowsManager.IsWindowOpen<QuestWindow>() || NewWindowsManager.IsWindowOpen<BuildingWindow>(out var building) && building.IsPreviewing);
#endif
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
        //JoyStick.enabled = false;
        //JoyStick.Stop();
    }

    public void Init()
    {

        DragableManager.Instance.ResetIcon();
        ProgressBar.Instance.Cancel();
    }
}
