using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPanel : SingletonWindow<InteractionPanel>,IHideable
{
    public RectTransform view;

    public GameObject buttonPrefab;
    public Transform buttonParent;

    public override bool IsOpen => true;

    public bool ScrollAble => buttons.Count > 1;

    public bool IsHidden { get; private set; }

    private readonly HashSet<IInteractive> objects = new HashSet<IInteractive>();
    private readonly Dictionary<IInteractive, InteractionButton> buttons = new Dictionary<IInteractive, InteractionButton>();

    private readonly Dictionary<IInteractive, bool> showStates = new Dictionary<IInteractive, bool>();

    private int selectIndex;

    #region UI相关
    public bool Insert(IInteractive interactive)
    {
        if (objects.Contains(interactive)) return false;
        objects.Add(interactive);
        InteractionButton button = ObjectPool.Get(buttonPrefab, buttonParent).GetComponent<InteractionButton>();
        button.Init(interactive);
        buttons.Add(interactive, button);
        if (buttonParent.childCount < 2)
        {
            selectIndex = button.transform.GetSiblingIndex();
            UpdateButtons();
        }
        StartCoroutine(DelayUpdateView());
        return true;
    }

    public void ShowOrHidePanelBy(IInteractive interactive, bool show)
    {
        if (interactive == null) return;
        if (show)
        {
            if (showStates.TryGetValue(interactive, out var state))
                PauseDisplay(state);
            showStates.Remove(interactive);
        }
        else
        {
            if (showStates.ContainsKey(interactive)) return;
            showStates.Add(interactive, IsHidden);
            PauseDisplay(true);
        }
    }

    private void PauseDisplay(bool v)
    {
        WindowsManager.HideWindow(this, v);
    }

    public bool Remove(IInteractive interactive)
    {
        if (buttons.TryGetValue(interactive, out var button))
        {
            int index = button.transform.GetSiblingIndex();
            button.Clear(true);
            buttons.Remove(interactive);
            objects.Remove(interactive);
            if (index == selectIndex)
                if (index > 0) Up();
                else UpdateButtons();
            return true;
        }
        return false;
    }

    public void Down()
    {
        if (selectIndex < buttonParent.transform.childCount - 1) selectIndex++;
        UpdateButtons();
        UpdateView();
    }
    public void Up()
    {
        if (selectIndex > 0) selectIndex--;
        UpdateButtons();
        UpdateView();
    }
    private void UpdateButtons()
    {
        foreach (var btn in buttons.Values)
        {
            btn.SetSelected(btn.transform.GetSiblingIndex() == selectIndex);
        }
    }
    private void UpdateView()
    {
        if (buttonParent.childCount < 4) return;
        var buttonTrans = buttonParent.GetChild(selectIndex) as RectTransform;
        //获取四个顶点的位置，顶点序号
        //  1 ┏━┓ 2
        //  0 ┗━┛ 3
        Vector3[] vCorners = new Vector3[4];
        view.GetWorldCorners(vCorners);
        Vector3[] bCorners = new Vector3[5];
        buttonTrans.GetWorldCorners(bCorners);
        if (bCorners[1].y > vCorners[1].y)//按钮上方被挡住
            buttonParent.position += Vector3.up * (vCorners[1].y - bCorners[1].y);
        if (bCorners[0].y < vCorners[0].y)//按钮下方被挡住
            buttonParent.position += Vector3.up * (vCorners[0].y - bCorners[0].y);
    }
    private IEnumerator DelayUpdateView()
    {
        yield return new WaitForEndOfFrame();
        UpdateView();
    }

    public void DoSelectInteract()
    {
        if (IsHidden || buttonParent.childCount < 1) return;
        var button = buttonParent.GetChild(selectIndex);
        if (button) button.GetComponent<Button>().onClick.Invoke();
    }

    protected override bool OnOpen(params object[] args)
    {
        return false;
    }
    protected override bool OnClose(params object[] args)
    {
        return false;
    }

    public void Hide(bool hide, params object[] args)
    {
        IHideable.HideHelper(content, hide);
        IsHidden = hide;
    }
    #endregion
}