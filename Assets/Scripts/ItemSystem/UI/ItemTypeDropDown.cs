using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace ZetanStudio.ItemSystem.UI
{
    [RequireComponent((typeof(Dropdown)))]
    public class ItemTypeDropDown : MonoBehaviour
    {
        public Dropdown Dropdown { get; private set; }
        public List<string> Types { get; private set; }
        public int Value { get => Dropdown.value; set => Dropdown.value = value; }

        private Action<int> callback;
        private Action<string> callback_string;
        public IFiltableItemContainer container;


        private void Awake()
        {
            Dropdown = GetComponent<Dropdown>();
            Dropdown.onValueChanged.RemoveAllListeners();
            Dropdown.ClearOptions();
            Dropdown.AddOptions(new List<string>() { "全部" });
            Types = new List<string>(ItemTypeEnum.Instance.GetUINames());
            Dropdown.AddOptions(Types);
            Dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        public void SetCallback(Action<int> callback)
        {
            this.callback = callback;
        }

        public void SetCallback(Action<string> callback)
        {
            callback_string = callback;
        }

        private void OnValueChanged(int index)
        {
            callback?.Invoke(index);
            callback_string?.Invoke(Types[index]);

            if (index == 0)
                container?.DoFilter(x => true);
            else
                container?.DoFilter(x => x & x.Type.Name == Types[index - 1]);
        }
    }
}
