using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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
        ZetanUtility.SetActive(gameObject, true);
        buttonName.text = name;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(clickAction);
        IsHiding = false;
    }

    public void Hide()
    {
        ZetanUtility.SetActive(gameObject, false);
        IsHiding = true;
    }
}
