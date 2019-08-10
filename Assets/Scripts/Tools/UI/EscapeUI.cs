using UnityEngine;
using UnityEngine.UI;

public class EscapeUI : MonoBehaviour
{
    public CanvasGroup escapeMenu;

    [HideInInspector]
    public Canvas menuCanvas;

    public Button closeButton;

    public Button exitButton;

    private void Awake()
    {
        if (!escapeMenu.GetComponent<GraphicRaycaster>()) escapeMenu.gameObject.AddComponent<GraphicRaycaster>();
        menuCanvas = escapeMenu.GetComponent<Canvas>();
        menuCanvas.overrideSorting = true;
        menuCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        closeButton.onClick.AddListener(EscapeMenuManager.Instance.CloseWindow);
        exitButton.onClick.AddListener(EscapeMenuManager.Instance.Exit);
    }
}
