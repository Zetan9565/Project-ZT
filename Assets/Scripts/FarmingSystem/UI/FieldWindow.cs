using UnityEngine;
using UnityEngine.UI;

public class FieldWindow : Window, IHideable
{
    public Field CurrentField { get; private set; }

    public bool IsHidden { get; protected set; }

    [SerializeField]
    private CropList cropList;

    public Text space;
    public Text fertility;
    public Text humidity;

    public Button plantButton;
    public Button workerButton;
    public Button destroyButton;

    protected override void OnAwake()
    {
        plantButton.onClick.AddListener(OpenClosePlantWindow);
        destroyButton.onClick.AddListener(DestroyCurrentField);
        workerButton.onClick.AddListener(DispatchWorker);
        cropList.SetItemModifier(x => x.SetWindow(this));
    }

    protected override bool OnOpen(params object[] args)
    {
        if (!PlayerManager.Instance.CheckIsNormalWithAlert())
            return false;
        CurrentField = openBy as Field;
        if (!CurrentField) return false;
        UpdateCropsArea();
        UpdateUI();
        return true;
    }

    protected override bool OnClose(params object[] args)
    {
        if (CurrentField) CurrentField.EndManagement();
        CurrentField = null;
        return true;
    }

    public void CancelManage()
    {
        Close();
    }

    public void UpdateUI()
    {
        space.text = CurrentField.Crops.Count + "/" + CurrentField.FData.spaceOccup;
        fertility.text = CurrentField.FData.fertility.ToString();
        humidity.text = CurrentField.FData.fertility.ToString();
        cropList.Refresh();
    }

    private void UpdateCropsArea()
    {
        cropList.Refresh(CurrentField.Crops);
    }

    public void DestroyCurrentField()
    {
        if (CurrentField) StructureManager.Instance.DestroyStructure(CurrentField.Data);
    }

    public void DispatchWorker()
    {
        MessageManager.Instance.New("敬请期待");
    }

    public void OnCropPlanted(object[] msg)
    {
        if (msg.Length > 0 && msg[0] is Field field && field == CurrentField && msg[1] is CropData)
            UpdateUI();
    }

    public void Remove(Crop crop)
    {
        if (!crop) return;
        crop.Parent.Remove(crop);
        cropList.Refresh();
    }

    public void OpenClosePlantWindow()
    {
        if (WindowsManager.IsWindowOpen<PlantWindow>(out var plant)) plant.Close();
        else WindowsManager.OpenWindow<PlantWindow>(CurrentField);
    }

    public void Hide(bool hide, params object[] args)
    {
        if (!IsOpen) return;
        IHideable.HideHelper(content, hide);
        IsHidden = hide;
    }

    protected override void RegisterNotify()
    {
        NotifyCenter.AddListener(FieldManager.FieldCropPlanted, OnCropPlanted, this);
    }
}