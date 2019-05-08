using LeoLuz.PlugAndPlayJoystick;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<UIManager>();
            return instance;
        }
    }

    [SerializeField]
    private Button questButton;

    [SerializeField]
    private Button backpackButton;

    [SerializeField]
    private Button buildingButton;

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

    private void Awake()
    {
#if UNITY_STANDALONE
        EnableJoyStick(false);
        MyTools.SetActive(JoyStick.gameObject, false);
        MyTools.SetActive(JoyStick.KnobBackground.gameObject, false);
#elif UNITY_ANDROID
        MyTools.SetActive(JoyStick.gameObject, true);
        MyTools.SetActive(JoyStick.KnobBackground.gameObject, true);
#endif
        MyTools.SetActive(InteractiveButton.gameObject, false);
        questButton.onClick.AddListener(QuestManager.Instance.OpenCloseWindow);
        backpackButton.onClick.AddListener(BackpackManager.Instance.OpenCloseWindow);
        buildingButton.onClick.AddListener(BuildingManager.Instance.OpenCloseWindow);
    }

    public void EnableJoyStick(bool value)
    {
        JoyStick.enabled = value && !(DialogueManager.Instance.IsUIOpen || ShopManager.Instance.IsUIOpen ||
            WarehouseManager.Instance.IsUIOpen || QuestManager.Instance.IsUIOpen || BuildingManager.Instance.IsUIOpen);
    }

    public void EnableInteractive(bool value, string name = null)
    {
#if UNITY_ANDROID
        if (!value)
            MyTools.SetActive(InteractiveButton.gameObject, value);
        else
        {
            MyTools.SetActive(InteractiveButton.gameObject, value &&
                (DialogueManager.Instance.TalkAble && !WarehouseManager.Instance.IsUIOpen ||
                WarehouseManager.Instance.StoreAble && !DialogueManager.Instance.IsUIOpen
                ));
        }
#endif
        if (!string.IsNullOrEmpty(name) && value)
        {
            MyTools.SetActive(interactiveName.transform.parent.gameObject, true);
            interactiveName.text = name;
        }
        else
        {
            MyTools.SetActive(interactiveName.transform.parent.gameObject, false);
            interactiveName.text = string.Empty;
        }
    }
}
