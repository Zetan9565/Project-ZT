using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TabbedBar : VisualElement
{
    public new class UxmlFactory : UxmlFactory<TabbedBar, UxmlTraits> { }
    public Color selectedColor = Color.clear;
    public Color normalColor = new Color(0, 0, 0, 0.25f);
    private Label selected;
    private Action<int> onSelectionChanged;
    public Action<int, ContextualMenuPopulateEvent> onRightClick;

    private int selectedIndex;
    public int SelectedIndex { get => selectedIndex; private set => SetSelected(value); }

    public TabbedBar() : base()
    {
        style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
    }

    public void Refresh(string[] tabs, Action<int> onSelectionChanged, float? radius = null, Color? selectedColor = null, Color? normalColor = null)
    {
        Clear();
        if (tabs == null || tabs.Length < 1)
        {
            Debug.LogError("至少需要一个页签");
            return;
        }
        for (int i = 0; i < tabs.Length; i++)
        {
            var tab = tabs[i];
            Label button = new Label();
            button.style.borderTopWidth = 1;
            button.style.borderTopColor = new Color(0, 0, 0, 0.5f);
            button.style.borderLeftWidth = 1;
            button.style.borderLeftColor = new Color(0, 0, 0, 0.5f);
            button.style.borderRightWidth = 1;
            button.style.borderRightColor = new Color(0, 0, 0, 0.5f);
            button.style.borderTopLeftRadius = radius ?? 5;
            button.style.borderTopRightRadius = radius ?? 5;
            button.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            button.style.backgroundColor = normalColor ?? this.normalColor;
            button.style.flexGrow = 1;
            button.text = tab;
            button.RegisterCallback(new EventCallback<ClickEvent>(e =>
            {
                OnTabClick(button);
            }));
            button.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                OnTabRightClick(button, evt);
            }));
            Add(button);
        }
        this.onSelectionChanged = onSelectionChanged;
        this.selectedColor = selectedColor ?? this.selectedColor;
        this.normalColor = normalColor ?? this.normalColor;
    }

    private void OnTabRightClick(Label button, ContextualMenuPopulateEvent evt)
    {
        onRightClick?.Invoke(IndexOf(button) + 1, evt);
    }

    public void SetSelected(int index)
    {
        if (ElementAt(index - 1) is Label tab)
            OnTabClick(tab);
    }

    private void OnTabClick(Label tab)
    {
        if (selected == tab) return;
        if (selected != null) DeselectTab(selected);
        selected = tab;
        selectedIndex = IndexOf(tab) + 1;
        SelectTab(tab);
        onSelectionChanged?.Invoke(selectedIndex);
    }
    private void SelectTab(Label tab)
    {
        tab.style.backgroundColor = selectedColor;
    }
    private void DeselectTab(Label tab)
    {
        tab.style.backgroundColor = normalColor;
    }
}