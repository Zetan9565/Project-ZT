using System;
using UnityEngine;
using UnityEngine.UI;

public class AmountWindow : Window
{
    [SerializeField]
    private Text titleText;
    [SerializeField]
    private InputField field;

    [SerializeField]
    private Button maxButton;
    [SerializeField]
    private Button clearButton;
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private Button confirmButton;
    [SerializeField]
    private Button plusButton;
    [SerializeField]
    private Button minusButton;

    [SerializeField]
    private Button[] numButtons;

    [SerializeField]
    private Vector2 defaultOffset = new Vector2(-60, 60);

    private RectTransform windowRect;
    private Action<long> onConfirm;
    private Action onCancel;

    private long amount;
    private long min;
    private long max;

    private bool firstInput;

    protected override void OnAwake()
    {
        windowRect = content.GetComponent<RectTransform>();
        field.onValueChanged.AddListener((s) => FixAmount());
        field.characterValidation = InputField.CharacterValidation.Integer;
        field.contentType = InputField.ContentType.IntegerNumber;
        field.characterLimit = 12;
        field.text = 0.ToString();
        maxButton.onClick.AddListener(Max);
        clearButton.onClick.AddListener(Clear);
        backButton.onClick.AddListener(Back);
        confirmButton.onClick.AddListener(Confirm);
        closeButton.onClick.AddListener(Cancel);
        plusButton.onClick.AddListener(Plus);
        minusButton.onClick.AddListener(Minus);
        for (int i = 0; i < numButtons.Length; i++)
        {
            int num = i;
            if (numButtons[i]) numButtons[i].onClick.AddListener(() => ClickNumber(num));
        }
    }

    public static AmountWindow StartInput(Action<long> confirmAction, long max, string title = "", Vector2? position = null, Vector2? offset = null)
    {
        return StartInput(confirmAction, null, 0, max, title, position, offset);
    }

    public static AmountWindow StartInput(Action<long> confirmAction, long min, long max, string title = "", Vector2? position = null, Vector2? offset = null)
    {
        return StartInput(confirmAction, null, min, max, title, position, offset);
    }

    public static AmountWindow StartInput(Action<long> confirmAction, Action cancelAction, long max, string title = "", Vector2? position = null, Vector2? offset = null)
    {
        return StartInput(confirmAction, cancelAction, 0, max, title, position, offset);
    }

    public static AmountWindow StartInput(Action<long> confirmAction, Action cancelAction, long min, long max, string title = "", Vector2? position = null, Vector2? offset = null)
    {
        if (min < 0 || max < 1) return null;
        return NewWindowsManager.OpenWindow<AmountWindow>(confirmAction, cancelAction, min, max, title, position, offset);
    }

    private void Refresh(string title)
    {
        field.shouldHideMobileInput = false;
        field.ActivateInputField();
        field.shouldHideMobileInput = true;
        if (string.IsNullOrEmpty(title)) titleText.text = "输入数量";
        else titleText.text = title;
        ZetanUtility.KeepInsideScreen(windowRect);
        firstInput = true;
    }

    public void ClickNumber(int num)
    {
        if (firstInput)
        {
            field.text = num.ToString();
            firstInput = false;
        }
        else if (field.text.Length < field.characterLimit - 1)
        {
            field.text += num;
            long.TryParse(field.text, out long current);
            if (current < min) current = min;
            else if (current > max) current = max;
            amount = current;
            field.text = amount.ToString();
        }
        field.MoveTextEnd(false);
    }

    public void Back()
    {
        field.text = field.text.Remove(field.text.Length - 1);
        long.TryParse(field.text, out long current);
        if (current < min) current = min;
        else if (current > max) current = max;
        amount = current;
        field.text = amount.ToString();
        field.MoveTextEnd(false);
        firstInput = false;
    }

    public void Plus()
    {
        long.TryParse(field.text, out long current);
        if (field.text.Length <= field.characterLimit - 1)
            if (current < max) current++;
            else current = max;
        amount = current;
        field.text = amount.ToString();
        field.MoveTextEnd(false);
        firstInput = false;
    }

    public void Minus()
    {
        long.TryParse(field.text, out long current);
        if (current > min && field.text.Length < field.characterLimit - 1) current--;
        else current = min;
        amount = current;
        field.text = amount.ToString();
        field.MoveTextEnd(false);
        firstInput = false;
    }

    public void Max()
    {
        amount = max;
        field.text = amount.ToString();
        field.MoveTextEnd(false);
        firstInput = false;
    }

    public void Clear()
    {
        amount = min;
        field.text = amount.ToString();
        field.MoveTextEnd(false);
        firstInput = false;
    }

    public void Confirm()
    {
        content.alpha = 0;
        content.blocksRaycasts = false;
        onConfirm?.Invoke(amount);
        firstInput = false;
    }

    public void Cancel()
    {
        content.alpha = 0;
        content.blocksRaycasts = false;
        onCancel?.Invoke();
        firstInput = false;
    }

    private void SetPosition(Vector2? position, Vector2? offset)
    {
        if (position.HasValue)
            if (offset.HasValue)
                windowRect.position = position.Value + offset.Value;
            else windowRect.position = position.Value + defaultOffset;
    }

    private void FixAmount()
    {
        long.TryParse(field.text, out long current);
        if (field.text.Length <= field.characterLimit - 1)
            if (current > max) current = max;
            else if (current < min) current = min;
        amount = current;
        field.text = amount.ToString();
        field.MoveTextEnd(false);
        if (amount < 1) confirmButton.interactable = false;
        else confirmButton.interactable = true;
    }

    protected override bool OnOpen(params object[] args)
    {
        if (args != null && args.Length > 3)
        {
            min = (long)args[2];
            max = (long)args[3];
            if (max < min)
            {
                max += min;
                min = max - min;
                max -= min;
            }
            if (min < 0 || max < 1) return false;
            amount = min <= 1 ? 1 : min;
            field.text = amount.ToString();
            onConfirm = args[0] as Action<long>;
            onCancel = args[1] as Action;
            SetPosition(args.Length > 5 ? args[5] as Vector2? : null, args.Length > 6 ? args[6] as Vector2? : null);
            Refresh(args.Length > 4 ? args[4] as string : null);
            return true;
        }
        return false;
    }
}