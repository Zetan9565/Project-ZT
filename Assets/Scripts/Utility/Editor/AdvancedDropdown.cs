using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public sealed class AdvancedDropdown<T> : AdvancedDropdown
{
    private readonly string title;
    private readonly List<T> values;
    private readonly Action<T> selectCallback;
    private readonly Func<T, string> nameGetter;
    private readonly Func<T, string> groupGetter;
    private readonly Func<T, Texture2D> iconGetter;

    public AdvancedDropdown(string title,
                            IEnumerable<T> values,
                            Action<T> selectCallback,
                            Func<T, string> nameGetter = null,
                            Func<T, string> groupGetter = null,
                            Func<T, Texture2D> iconGetter = null,
                            Vector2? winSize = null) : base(new AdvancedDropdownState())
    {
        this.title = title;
        this.values = new List<T>(values);
        if (groupGetter != null)
            this.values.Sort((x, y) =>
            {
                string nameX = groupGetter(x) + (nameGetter?.Invoke(x) ?? x.ToString());
                string nameY = groupGetter(y) + (nameGetter?.Invoke(y) ?? y.ToString());
                return string.Compare(nameX, nameY);
            });
        else this.values.Sort((x, y) => string.Compare(nameGetter?.Invoke(x) ?? x.ToString(), nameGetter?.Invoke(y) ?? y.ToString()));
        this.selectCallback = selectCallback;
        this.nameGetter = nameGetter;
        this.groupGetter = groupGetter;
        this.iconGetter = iconGetter;
        minimumSize = winSize ?? new Vector2(250, 250);
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem(title);
        Dictionary<string, AdvancedDropdownItem> groups = new Dictionary<string, AdvancedDropdownItem>();
        foreach (var value in values)
        {
            AdvancedDropdownItem parent = root;
            string groupStr = groupGetter?.Invoke(value);
            if (!string.IsNullOrEmpty(groupStr))
            {
                string[] groupContent = groupStr.Split('/');
                foreach (var groupName in groupContent)
                {
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        if (!groups.TryGetValue(groupName, out var group))
                        {
                            group = new AdvancedDropdownItem(groupName);
                            groups.Add(groupName, group);
                            parent.AddChild(group);
                        }
                        parent = group;
                    }
                }
            }
            parent.AddChild(new AdvancedDropdownItem<T>(nameGetter(value), value) { icon = iconGetter?.Invoke(value) });
        }
        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        if (item is AdvancedDropdownItem<T> tItem)
        {
            selectCallback?.Invoke(tItem.userData);
        }
    }

    private sealed class AdvancedDropdownItem<TData> : AdvancedDropdownItem
    {
        public readonly TData userData;

        public AdvancedDropdownItem(string name, TData userData) : base(name)
        {
            this.userData = userData;
        }
    }
}