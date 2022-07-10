using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.Extension;

namespace ZetanStudio.UI
{
    [DefaultExecutionOrder(-1), RequireComponent(typeof(ToggleGroup))]
    public class TabbedBar : MonoBehaviour
    {
        [SerializeField]
        private Toggle tabPrefab;
        [SerializeField]
        private RectTransform barTransform;

        private ToggleGroup group;
        private readonly List<Toggle> toggles = new List<Toggle>();
        private SimplePool<Toggle> pool;

        private Action<int> onSwitch;

        private int selectedIndex = 1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set => SetIndex(value);
        }

        private void Awake()
        {
            if (!barTransform) barTransform = this.GetOrAddComponent<RectTransform>();
            group = GetComponent<ToggleGroup>();
            group.allowSwitchOff = false;
            var toggles = this.GetComponentsInChildrenInOrder<Toggle>();
            for (int i = 0; i < toggles.Length; i++)
            {
                var toggle = toggles[i];
                this.toggles.Add(toggle);
                toggle.SetIsOnWithoutNotify(false);
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(isOn => { if (isOn) OnSwitch(toggle.transform.GetSiblingIndex()); });
                toggle.group = group;
                if (toggle.transform.GetSiblingIndex() == 0) toggle.SetIsOnWithoutNotify(true);
                else toggle.SetIsOnWithoutNotify(false);
            }
            group.EnsureValidState();
            pool = new SimplePool<Toggle>(tabPrefab, barTransform, 20);
        }

        private void OnSwitch(int index)
        {
            if (index < 0 || index >= toggles.Count) return;
            if (toggles[index].isOn)
            {
                selectedIndex = index + 1;
                onSwitch?.Invoke(selectedIndex);
            }
        }

        public void SetIndex(int index)
        {
            if (index < 1 || index > toggles.Count) return;
            toggles[index - 1].isOn = true;
            group.NotifyToggleOn(toggles[index - 1]);
        }

        public void Refresh(Action<int> onSwitch, List<string> texts = null)
        {
            this.onSwitch = onSwitch;
            if (texts != null)
            {
                while (toggles.Count < texts.Count)
                {
                    var toggle = pool.Get(barTransform);
                    toggle.SetIsOnWithoutNotify(false);
                    toggle.group = group;
                    toggle.onValueChanged.RemoveAllListeners();
                    toggle.onValueChanged.AddListener(isOn => { if (isOn) OnSwitch(toggle.transform.GetSiblingIndex()); });
                    toggles.Add(toggle);
                }
                while (toggles.Count > texts.Count)
                {
                    var toggle = toggles[^1];
                    toggle.group = null;
                    toggle.SetIsOnWithoutNotify(false);
                    toggle.onValueChanged.RemoveAllListeners();
                    pool.Put(toggle);
                }
                foreach (var toggle in toggles)
                {
                    if (toggle.transform.GetSiblingIndex() == 0) toggle.SetIsOnWithoutNotify(true);
                    else toggle.SetIsOnWithoutNotify(false);
                }
                group.EnsureValidState();
            }
        }
    }
}