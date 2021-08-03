using System;
using UnityEngine;

public class AmountManager : SingletonMonoBehaviour<AmountManager>
{
    [SerializeField]
    private AmountUI UI;

    [SerializeField]
    private Vector2 defaultOffset = new Vector2(-100, 100);

    public bool IsUIOpen
    {
        get
        {
            if (!UI || !UI.gameObject) return false;
            else if (UI.window.alpha > 0) return true;
            else return false;
        }
    }

    private Action<long> onConfirm;
    private Action onCancel;

    private long amount;
    private long min;
    private long max;

    private bool firstInput;

    public void New(Action<long> confirmAction, long max, string title = "")
    {
        long min = 0;
        if (max < min)
        {
            max = max + min;
            min = max - min;
            max = max - min;
        }
        this.max = max;
        this.min = min;
        amount = max > 0 ? 1 : 0;
        UI.amount.text = amount.ToString();
        onConfirm = confirmAction;
        onCancel = null;
        ShowUI(title);
    }

    public void New(Action<long> confirmAction, long min, long max, string title = "")
    {
        if (max < min)
        {
            max = max + min;
            min = max - min;
            max = max - min;
        }
        this.max = max;
        this.min = min;
        amount = max > min ? min : 0;
        UI.amount.text = amount.ToString();
        onConfirm = confirmAction;
        onCancel = null;
        ShowUI(title);
    }

    public void New(Action<long> confirmAction, Action cancelAction, long max, string title = "")
    {
        if (max < min)
        {
            max = max + min;
            min = max - min;
            max = max - min;
        }
        this.max = max;
        min = 0;
        amount = max >= 0 ? 0 : min;
        UI.amount.text = amount.ToString();
        onConfirm = confirmAction;
        onCancel = cancelAction;
        ShowUI(title);
    }

    public void New(Action<long> confirmAction, Action cancelAction, long max, long min, string title = "")
    {
        if (max < min)
        {
            max = max + min;
            min = max - min;
            max = max - min;
        }
        this.max = max;
        this.min = min;
        amount = max >= 0 ? 0 : min;
        UI.amount.text = amount.ToString();
        onConfirm = confirmAction;
        onCancel = cancelAction;
        ShowUI(title);
    }

    private void ShowUI(string title)
    {
        UI.amount.shouldHideMobileInput = false;
        UI.amount.ActivateInputField();
        UI.amount.shouldHideMobileInput = true;
        UI.windowCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        UI.window.alpha = 1;
        UI.window.blocksRaycasts = true;
        if (string.IsNullOrEmpty(title)) UI.title.text = "输入数量";
        else UI.title.text = title;
        ZetanUtility.KeepInsideScreen(UI.windowRect);
        firstInput = true;
    }

    public void ClickNumber(int num)
    {
        if(firstInput)
        {
            UI.amount.text = num.ToString();
            firstInput = false;
        }
        else if (UI.amount.text.Length < UI.amount.characterLimit - 1)
        {
            UI.amount.text += num;
            long.TryParse(UI.amount.text, out long current);
            if (current < min) current = min;
            else if (current > max) current = max;
            amount = current;
            UI.amount.text = amount.ToString();
        }
        UI.amount.MoveTextEnd(false);
    }

    public void Back()
    {
        UI.amount.text = UI.amount.text.Remove(UI.amount.text.Length - 1);
        long.TryParse(UI.amount.text, out long current);
        if (current < min) current = min;
        else if (current > max) current = max;
        amount = current;
        UI.amount.text = amount.ToString();
        UI.amount.MoveTextEnd(false);
        firstInput = false;
    }

    public void Plus()
    {
        long.TryParse(UI.amount.text, out long current);
        if (UI.amount.text.Length <= UI.amount.characterLimit - 1)
            if (current < max) current++;
            else current = max;
        amount = current;
        UI.amount.text = amount.ToString();
        UI.amount.MoveTextEnd(false);
        firstInput = false;
    }

    public void Minus()
    {
        long.TryParse(UI.amount.text, out long current);
        if (current > min && UI.amount.text.Length < UI.amount.characterLimit - 1) current--;
        else current = min;
        amount = current;
        UI.amount.text = amount.ToString();
        UI.amount.MoveTextEnd(false);
        firstInput = false;
    }

    public void Max()
    {
        amount = max;
        UI.amount.text = amount.ToString();
        UI.amount.MoveTextEnd(false);
        firstInput = false;
    }

    public void Clear()
    {
        amount = min;
        UI.amount.text = amount.ToString();
        UI.amount.MoveTextEnd(false);
        firstInput = false;
    }

    public void Confirm()
    {
        UI.window.alpha = 0;
        UI.window.blocksRaycasts = false;
        onConfirm?.Invoke(amount);
        firstInput = false;
    }

    public void Cancel()
    {
        UI.window.alpha = 0;
        UI.window.blocksRaycasts = false;
        onCancel?.Invoke();
        firstInput = false;
    }

    public void SetPosition(Vector2 targetPos, Vector2 offset)
    {
        UI.windowRect.position = targetPos + offset;
        ZetanUtility.KeepInsideScreen(UI.windowRect);
    }

    public void SetPosition(Vector2 targetPos)
    {
        UI.window.GetComponent<RectTransform>().position = targetPos + defaultOffset;
        ZetanUtility.KeepInsideScreen(UI.windowRect);
    }

    public void FixAmount()
    {
        //UI.amount.text = System.Text.RegularExpressions.Regex.Replace(UI.amount.text, @"[^0-9]+", "");
        long.TryParse(UI.amount.text, out long current);
        if (UI.amount.text.Length <= UI.amount.characterLimit - 1)
            if (current > max) current = max;
            else if (current < min) current = min;
        amount = current;
        UI.amount.text = amount.ToString();
        UI.amount.MoveTextEnd(false);
        if (amount < 1) UI.confirm.interactable = false;
        else UI.confirm.interactable = true;
    }
}