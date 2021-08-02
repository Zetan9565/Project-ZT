using System.Collections;
using System.Collections.Generic;
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

    private InteractiveObject interactiveObj;
    private Interactive interactiveCom;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Interact);
    }

    public void Init(InteractiveObject interactive)
    {
        interactiveObj = interactive;
        buttonText.text = interactive.name;
        buttonIcon.overrideSprite = interactive.Icon;
        SetSelected(false);
    }

    public void Init(Interactive interactive)
    {
        interactiveCom = interactive;
        buttonText.text = interactive.name;
        buttonIcon.overrideSprite = interactive.Icon;
        SetSelected(false);
    }

    private void Interact()
    {
        if (interactiveCom)
        {
            if (interactiveCom.DoInteract())
            {
                InteractionManager.Instance.Remove(interactiveCom);
            }
        }
        else if (interactiveObj.DoInteract())
        {
            InteractionManager.Instance.Remove(interactiveObj);
        }
    }

    public void Clear(bool recycle = false)
    {
        interactiveObj = null;
        interactiveCom = null;
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
