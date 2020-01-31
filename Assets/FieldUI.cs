using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FieldUI : MonoBehaviour
{
    public CanvasGroup fieldWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public GameObject cropPrefab;
    public Transform cropCellsParent;

    public Text space;
    public Text fertility;
    public Text humidity;

    public Button closeButton;
    public Button plantButton;
    public Button workerButton;
    public Button destroyButton;

    private void Awake()
    {
        if (!fieldWindow.GetComponent<GraphicRaycaster>()) fieldWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = fieldWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        closeButton.onClick.AddListener(FieldManager.Instance.CloseWindow);
        plantButton.onClick.AddListener(FieldManager.Instance.OpenClosePlantWindow);
        destroyButton.onClick.AddListener(FieldManager.Instance.DestroyCurrentField);
    }
}
