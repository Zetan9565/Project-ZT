using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ZetanStudio.Editor
{
    public sealed class AdvancedDropdown<T> : AdvancedDropdown
    {
        public string title;
        public List<T> values;
        public Action<T> selectCallback;
        public Func<T, string> nameGetter;
        public Func<T, string> tooltipGetter;
        public Func<T, string> groupGetter;
        public Func<T, Texture2D> iconGetter;
        public (string text, Action callback) noneCallback;
        public (string text, Action callback)[] addCallbacks;
        public bool displayNone;

        public AdvancedDropdown(IEnumerable<T> values, Action<T> selectCallback, Func<T, string> nameGetter = null,
                                Func<T, string> groupGetter = null, Func<T, Texture2D> iconGetter = null,
                                Func<T, string> tooltipGetter = null, (string, Action) noneCallback = default,
                                Comparison<T> sorter = null,
                                string title = null,
                                params (string, Action)[] addCallbacks) : this(new Vector2(50, 250), values, selectCallback, nameGetter, groupGetter, iconGetter, tooltipGetter, noneCallback, sorter, title, addCallbacks)
        { }
        public AdvancedDropdown(Vector2 windowSize, IEnumerable<T> values, Action<T> selectCallback, Func<T, string> nameGetter = null,
                                Func<T, string> groupGetter = null, Func<T, Texture2D> iconGetter = null,
                                Func<T, string> tooltipGetter = null, (string, Action) noneCallback = default,
                                Comparison<T> sorter = null,
                                string title = null,
                                params (string, Action)[] addCallbacks) : base(new AdvancedDropdownState())
        {
            this.title = string.IsNullOrEmpty(title) ? typeof(T).Name : title;
            this.values = new List<T>(values);
            if (sorter != null) this.values.Sort(sorter);
            else if (groupGetter != null)
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
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)) && iconGetter == null)
                iconGetter = o => { return Utility.Editor.GetIconForObject(o as UnityEngine.Object); };
            this.iconGetter = iconGetter;
            this.tooltipGetter = tooltipGetter;
            this.noneCallback = noneCallback;
            displayNone = noneCallback != default;
            this.addCallbacks = addCallbacks;
            minimumSize = windowSize;
        }

        public void Show(Vector2 position, float? width = null, float? height = null)
        {
            Show(new Rect(position.x, position.y, width ?? minimumSize.x, height ?? 0));
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(title);
            HashSet<string> existNames = new HashSet<string>();
            Dictionary<string, AdvancedDropdownItem> groups = new Dictionary<string, AdvancedDropdownItem>();
            if (displayNone)
                if (noneCallback != default)
                    root.AddChild(new AdvancedDropdownItem<Action>($"{(!string.IsNullOrEmpty(noneCallback.text) ? noneCallback.text : L10n.Tr("None"))}", string.Empty, noneCallback.callback));
                else root.AddChild(new AdvancedDropdownItem<T>($"({L10n.Tr("None")})", string.Empty, default));
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
                string name = nameGetter?.Invoke(value) ?? $"{value}";
                int num = 1;
                string uniueName = name;
                while (existNames.Contains($"{groupStr ?? string.Empty}-{uniueName}"))
                {
                    uniueName = $"{name} ({L10n.Tr("Repeat")} {num})";
                    num++;
                }
                existNames.Add($"{groupStr ?? string.Empty}-{uniueName}");
                string tooltip = tooltipGetter?.Invoke(value);
                if (string.IsNullOrEmpty(tooltip) || tooltip == name) tooltip = $"{L10n.Tr("Name")}: {name}";
                parent.AddChild(new AdvancedDropdownItem<T>(uniueName, tooltip, value) { icon = iconGetter?.Invoke(value) });
            }
            if (addCallbacks != null && !Array.TrueForAll(addCallbacks, x => x == default))
            {
                root.AddSeparator();
                var newg = new AdvancedDropdownItem(L10n.Tr("Create"));
                root.AddChild(newg);
                foreach (var add in addCallbacks)
                {
                    if (add != default)
                        newg.AddChild(new AdvancedDropdownItem<Action>($"{L10n.Tr("Create")} {add.text}", null, add.callback)
                        { icon = EditorGUIUtility.FindTexture("CreateAddNew") });
                }
            }
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is AdvancedDropdownItem<Action> aItem) aItem.userData?.Invoke();
            else if (item is AdvancedDropdownItem<T> tItem) selectCallback?.Invoke(tItem.userData);
        }

        private sealed class AdvancedDropdownItem<TData> : AdvancedDropdownItem
        {
            public readonly TData userData;

            public AdvancedDropdownItem(string name, string tooltip, TData userData) : base(name)
            {
                this.userData = userData;
                if (!string.IsNullOrEmpty(tooltip)) typeof(AdvancedDropdownItem).GetProperty("tooltip", Utility.CommonBindingFlags).SetValue(this, tooltip);
            }
        }
    }
}