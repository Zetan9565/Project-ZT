using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InteractionButton : MonoBehaviour
{
    [SerializeField]
    private Image buttonIcon;
    [SerializeField]
    private Text buttonText;
    [SerializeField]
    private GameObject selectMark;

    private Button button;

    private IInteractive interactive;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Interact);
    }

    public void Init(IInteractive interactive)
    {
        this.interactive = interactive;
        buttonText.text = interactive.Name;
        buttonIcon.overrideSprite = interactive.Icon;
        SetSelected(false);
    }

    private void Interact()
    {
        if (interactive?.DoInteract() ?? false)
        {
            InteractionPanel.Instance.Remove(interactive);
        }
    }

    public void Clear(bool recycle = false)
    {
        interactive = null;
        SetSelected(false);
        buttonText.text = string.Empty;
        buttonIcon.overrideSprite = null;
        if (recycle) ObjectPool.Put(gameObject);
    }

    public void SetSelected(bool value)
    {
        ZetanUtility.SetActive(selectMark, value);
    }
}
