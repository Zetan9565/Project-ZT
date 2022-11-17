using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ZetanStudio.UI
{
    [RequireComponent(typeof(Button))]
    public class TipsButton : MonoBehaviour
    {
        [SerializeField]
        public Text buttonName;
        public new string name => buttonName.text;

        private Button button;

        public bool IsHiding { get; private set; }

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void Show(string name, UnityAction clickAction)
        {
            Utility.SetActive(gameObject, true);
            buttonName.text = name;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(clickAction);
            IsHiding = false;
        }

        public void Hide()
        {
            Utility.SetActive(gameObject, false);
            IsHiding = true;
        }
    }
}