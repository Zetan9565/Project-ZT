using UnityEngine;
using UnityEngine.Events;

public class AmountManager : SingletonMonoBehaviour<AmountManager>
{
    [SerializeField]
    private AmountUI UI;

    [SerializeField]
    private Vector2 defaultOffset = new Vector2(-100, 100);

    public long Amount { get; private set; }

    public bool IsUIOpen
    {
        get
        {
            if (!UI || !UI.gameObject) return false;
            else if (UI.window.alpha > 0) return true;
            else return false;
        }
    }

    private readonly UnityEvent onConfirm = new UnityEvent();
    private readonly UnityEvent onCancel = new UnityEvent();

    private long min;
    private long max;

    public void New(UnityAction confirmAction, long max, string title = "")
    {
        if (max < min)
        {
            max = max + min;
            min = max - min;
            max = max - min;
        }
        this.max = max;
        min = 0;
        Amount = max >= 0 ? 0 : min;
        UI.amount.text = Amount.ToString();
        onConfirm.RemoveAllListeners();
        if (confirmAction != null) onConfirm.AddListener(confirmAction);
        UI.windowCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        UI.window.alpha = 1;
        UI.window.blocksRaycasts = true;
        if (string.IsNullOrEmpty(title)) UI.title.text = "输入数量";
        else UI.title.text = title;
        ZetanUtility.KeepInsideScreen(UI.windowRect);
    }

    public void New(UnityAction confirmAction, long max, long min, string title = "")
    {
        if (max < min)
        {
            max = max + min;
            min = max - min;
            max = max - min;
        }
        this.max = max;
        this.min = min;
        Amount = max >= 0 ? 0 : min;
        UI.amount.text = Amount.ToString();
        onConfirm.RemoveAllListeners();
        if (confirmAction != null) onConfirm.AddListener(confirmAction);
        UI.windowCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        UI.window.alpha = 1;
        UI.window.blocksRaycasts = true;
        if (string.IsNullOrEmpty(title)) UI.title.text = "输入数量";
        else UI.title.text = title;
        ZetanUtility.KeepInsideScreen(UI.windowRect);
    }

    public void ClickNumber(int num)
    {
        if (UI.amount.text.Length < UI.amount.characterLimit - 1)
        {
            UI.amount.text += num;
            long.TryParse(UI.amount.text, out long current);
            if (current < min) current = min;
            else if (current > max) current = max;
            Amount = current;
            UI.amount.text = Amount.ToString();
        }
        UI.amount.MoveTextEnd(false);
    }

    public void Back()
    {
        UI.amount.text = UI.amount.text.Remove(UI.amount.text.Length - 1);
        long.TryParse(UI.amount.text, out long current);
        if (current < min) current = min;
        else if (current > max) current = max;
        Amount = current;
        UI.amount.text = Amount.ToString();
        UI.amount.MoveTextEnd(false);
    }

    public void Plus()
    {
        long.TryParse(UI.amount.text, out long current);
        if (UI.amount.text.Length <= UI.amount.characterLimit - 1)
            if (current < max) current++;
            else current = max;
        Amount = current;
        UI.amount.text = Amount.ToString();
        UI.amount.MoveTextEnd(false);
    }

    public void Minus()
    {
        long.TryParse(UI.amount.text, out long current);
        if (current > min && UI.amount.text.Length < UI.amount.characterLimit - 1) current--;
        else current = min;
        Amount = current;
        UI.amount.text = Amount.ToString();
        UI.amount.MoveTextEnd(false);
    }

    public void Max()
    {
        Amount = max;
        UI.amount.text = Amount.ToString();
        UI.amount.MoveTextEnd(false);
    }

    public void Clear()
    {
        Amount = min;
        UI.amount.text = Amount.ToString();
        UI.amount.MoveTextEnd(false);
    }

    public void Confirm()
    {
        UI.window.alpha = 0;
        UI.window.blocksRaycasts = false;
        onConfirm?.Invoke();
        onConfirm.RemoveAllListeners();
        onCancel.RemoveAllListeners();
    }

    public void Cancel()
    {
        UI.window.alpha = 0;
        UI.window.blocksRaycasts = false;
        onCancel?.Invoke();
        onConfirm.RemoveAllListeners();
        onCancel.RemoveAllListeners();
    }

    public void SetPosition(Vector2 targetPos, Vector2 offset)
    {
        UI.windowRect.position = targetPos + offset;
        ZetanUtility.KeepInsideScreen(UI.windowRect);
    }

    public void SetPosition(Vector2 target)
    {
        UI.window.GetComponent<RectTransform>().position = target + defaultOffset;
        ZetanUtility.KeepInsideScreen(UI.windowRect);
    }

    public void FixAmount()
    {
        UI.amount.text = System.Text.RegularExpressions.Regex.Replace(UI.amount.text, @"[^0-9]+", "");
        long.TryParse(UI.amount.text, out long current);
        if (UI.amount.text.Length <= UI.amount.characterLimit - 1)
            if (current > max) current = max;
            else if (current < min) current = min;
        Amount = current;
        UI.amount.text = Amount.ToString();
        UI.amount.MoveTextEnd(false);
        if (Amount < 1) UI.confirm.interactable = false;
        else UI.confirm.interactable = true;
    }
}