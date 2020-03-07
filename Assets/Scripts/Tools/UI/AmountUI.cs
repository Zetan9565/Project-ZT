using UnityEngine;
using UnityEngine.UI;

public class AmountUI : WindowUI
{
    [HideInInspector]
    public RectTransform windowRect;

    public Text title;
    public InputField amount;

    public Button max;
    public Button clear;
    public Button back;
    public Button confirm;
    public Button plus;
    public Button minus;

    public Button[] numButtons;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        windowRect = window.GetComponent<RectTransform>();
        amount.onValueChanged.AddListener(delegate { AmountManager.Instance.FixAmount(); });
        max.onClick.AddListener(AmountManager.Instance.Max);
        clear.onClick.AddListener(AmountManager.Instance.Clear);
        back.onClick.AddListener(AmountManager.Instance.Back);
        confirm.onClick.AddListener(AmountManager.Instance.Confirm);
        closeButton.onClick.AddListener(AmountManager.Instance.Cancel);
        plus.onClick.AddListener(AmountManager.Instance.Plus);
        minus.onClick.AddListener(AmountManager.Instance.Minus);
        amount.characterLimit = 12;
        amount.text = 0.ToString();
        for (int i = 0; i < numButtons.Length; i++)
        {
            int num = i;
            if (numButtons[i]) numButtons[i].onClick.AddListener(delegate { AmountManager.Instance.ClickNumber(num); });
        }
    }
}
