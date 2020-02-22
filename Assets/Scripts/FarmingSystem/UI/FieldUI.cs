using UnityEngine;
using UnityEngine.UI;

public class FieldUI : WindowUI
{
    public GameObject cropPrefab;
    public Transform cropCellsParent;

    public Text space;
    public Text fertility;
    public Text humidity;

    public Button plantButton;
    public Button workerButton;
    public Button destroyButton;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(FieldManager.Instance.CloseWindow);
        plantButton.onClick.AddListener(FieldManager.Instance.OpenClosePlantWindow);
        destroyButton.onClick.AddListener(FieldManager.Instance.DestroyCurrentField);
    }
}
