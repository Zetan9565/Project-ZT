using UnityEngine;
using UnityEngine.UI;

public class NewItemWindowUI : ItemWindowBaseUI
{
    public ItemWindowBaseUI subUI;

    public Button mulFunButton;

    public Button discardButton;

    public GameObject buttonsArea;

    [HideInInspector]
    private CanvasGroup buttonAreaCanvas;

    private new void Awake()
    {
        base.Awake();
        ZetanUtility.SetActive(closeButton.gameObject, false);
#if UNITY_STANDALONE
        ZetanUtility.SetActive(buttonsArea, false);
#elif UNITY_ANDROID
        if (!buttonsArea.GetComponent<CanvasGroup>()) buttonAreaCanvas = buttonsArea.AddComponent<CanvasGroup>();
        buttonAreaCanvas.ignoreParentGroups = true;
        discardButton.onClick.AddListener(ItemWindowManager.Instance.DiscardCurrentItem);
        closeButton.onClick.AddListener(ItemWindowManager.Instance.CloseWindow);
#endif
    }
}