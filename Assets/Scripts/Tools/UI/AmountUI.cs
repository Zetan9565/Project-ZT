using UnityEngine;
using UnityEngine.UI;

public class AmountUI : MonoBehaviour
{
    public CanvasGroup amountWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public InputField amount;

    public Button max;
    public Button clear;
    public Button cancel;
    public Button confirm;
    public Button plus;
    public Button minus;

    public Button[] numButtons;

    // Start is called before the first frame update
    void Awake()
    {
        if (!amountWindow.GetComponent<GraphicRaycaster>()) amountWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = amountWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        amount.onValueChanged.AddListener(delegate { AmountHandler.Instance.FixAmount(); });
        max.onClick.AddListener(AmountHandler.Instance.Max);
        clear.onClick.AddListener(AmountHandler.Instance.Clear);
        cancel.onClick.AddListener(AmountHandler.Instance.Cancel);
        plus.onClick.AddListener(AmountHandler.Instance.Plus);
        minus.onClick.AddListener(AmountHandler.Instance.Minus);
        amount.characterLimit = 12;
        amount.text = 0.ToString();
        for (int i = 0; i < numButtons.Length; i++)
        {
            int num = i;
            if (numButtons[i]) numButtons[i].onClick.AddListener(delegate { AmountHandler.Instance.Number(num); });
        }
    }
}
