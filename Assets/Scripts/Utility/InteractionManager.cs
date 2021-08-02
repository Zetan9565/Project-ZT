using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : WindowHandler<InteractionUI, InteractionManager>
{
    public override bool IsUIOpen => true;

    public bool ScrollAble => buttons.Count + buttons2.Count > 3;

    private readonly HashSet<InteractiveObject> objects = new HashSet<InteractiveObject>();
    private readonly Dictionary<InteractiveObject, InteractionButton> buttons = new Dictionary<InteractiveObject, InteractionButton>();

    private readonly HashSet<Interactive> objects2 = new HashSet<Interactive>();
    private readonly Dictionary<Interactive, InteractionButton> buttons2 = new Dictionary<Interactive, InteractionButton>();

    private readonly Dictionary<InteractiveObject, bool> showStates = new Dictionary<InteractiveObject, bool>();
    private readonly Dictionary<Interactive, bool> showStates2 = new Dictionary<Interactive, bool>();

    private int selectIndex;

    #region UI相关
    public bool Insert(InteractiveObject interactive)
    {
        if (objects.Contains(interactive)) return false;
        objects.Add(interactive);
        InteractionButton button = ObjectPool.Get(UI.buttonPrefab, UI.buttonParent).GetComponent<InteractionButton>();
        button.Init(interactive);
        buttons.Add(interactive, button);
        if (UI.buttonParent.childCount < 2)
        {
            selectIndex = button.transform.GetSiblingIndex();
            UpdateButtons();
        }
        StartCoroutine(DelayUpdateView());
        return true;
    }

    public void ShowOrHidePanelBy(InteractiveObject interactive, bool show)
    {
        if (show)
        {
            if (showStates.TryGetValue(interactive, out var state))
                PauseDisplay(state);
            showStates.Remove(interactive);
        }
        else
        {
            if (showStates.ContainsKey(interactive)) return;
            showStates.Add(interactive, IsPausing);
            PauseDisplay(true);
        }
    }
    public void ShowOrHidePanelBy(Interactive interactive, bool show)
    {
        if (show)
        {
            if (showStates2.TryGetValue(interactive, out var state))
                PauseDisplay(state);
            showStates2.Remove(interactive);
        }
        else
        {
            if (showStates2.ContainsKey(interactive)) return;
            showStates2.Add(interactive, IsPausing);
            PauseDisplay(true);
        }
    }

    public bool Insert(Interactive interactive)
    {
        if (objects2.Contains(interactive)) return false;
        objects2.Add(interactive);
        InteractionButton button = ObjectPool.Get(UI.buttonPrefab, UI.buttonParent).GetComponent<InteractionButton>();
        button.Init(interactive);
        buttons2.Add(interactive, button);
        if (UI.buttonParent.childCount < 2)
        {
            selectIndex = button.transform.GetSiblingIndex();
            UpdateButtons();
        }
        StartCoroutine(DelayUpdateView());
        return true;
    }

    public bool Remove(InteractiveObject interactive)
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
    public bool Remove(Interactive interactive)
    {
        if (buttons2.TryGetValue(interactive, out var button))
        {
            int index = button.transform.GetSiblingIndex();
            button.Clear(true);
            buttons2.Remove(interactive);
            objects2.Remove(interactive);
            if (index == selectIndex)
                if (index > 0) Up();
                else UpdateButtons();
            return true;
        }
        return false;
    }

    public void Down()
    {
        if (selectIndex < UI.buttonParent.transform.childCount - 1) selectIndex++;
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
        foreach (var btn in buttons2.Values)
        {
            btn.SetSelected(btn.transform.GetSiblingIndex() == selectIndex);
        }
    }
    private void UpdateView()
    {
        if (UI.buttonParent.childCount < 4) return;
        var buttonTrans = UI.buttonParent.GetChild(selectIndex) as RectTransform;
        //获取四个顶点的位置，顶点序号
        //  1 ┏━┓ 2
        //  0 ┗━┛ 3
        Vector3[] vCorners = new Vector3[4];
        UI.view.GetWorldCorners(vCorners);
        Vector3[] bCorners = new Vector3[5];
        buttonTrans.GetWorldCorners(bCorners);
        if (bCorners[1].y > vCorners[1].y)//按钮上方被挡住
            UI.buttonParent.position += Vector3.up * (vCorners[1].y - bCorners[1].y);
        if (bCorners[0].y < vCorners[0].y)//按钮下方被挡住
            UI.buttonParent.position += Vector3.up * (vCorners[0].y - bCorners[0].y);
    }
    private IEnumerator DelayUpdateView()
    {
        yield return new WaitForEndOfFrame();
        UpdateView();
    }

    public void DoSelectInteract()
    {
        if (IsPausing || UI.buttonParent.childCount < 1) return;
        var button = UI.buttonParent.GetChild(selectIndex);
        if (button) button.GetComponent<Button>().onClick.Invoke();
    }

    public override void OpenWindow() { }//不需要基类这个方法，屏蔽
    public override void CloseWindow() { }//同上
    public override void SetUI(InteractionUI UI)
    {
        objects.Clear();
        foreach (var btn in buttons.Values)
        {
            btn.Clear(true);
        }
        buttons.Clear();

        objects2.Clear();
        foreach (var btn in buttons2.Values)
        {
            btn.Clear(true);
        }
        buttons2.Clear();
        this.UI = UI;
    }
    #endregion
}