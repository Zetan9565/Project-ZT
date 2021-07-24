using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("ZetanStudio/管理器/农田管理器")]
public class FieldManager : WindowHandler<FieldUI, FieldManager>
{
    public Field CurrentField { get; private set; }

    private readonly List<CropAgent> cropAgents = new List<CropAgent>();

    public bool IsManaging { get; private set; }

    public override void OpenWindow()
    {
        if (!CurrentField) return;
        base.OpenWindow();
        if (!IsUIOpen) return;
        UpdateCropsArea();
        UpdateUI();
        UIManager.Instance.EnableJoyStick(false);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        IsManaging = false;
        if (CurrentField) CurrentField.OnDoneManage();
        CurrentField = null;
        UIManager.Instance.EnableJoyStick(true);
    }

    public void OpenClosePlantWindow()
    {
        if (!PlantManager.Instance.IsUIOpen)
        {
            PlantManager.Instance.Init(CurrentField);
            PlantManager.Instance.OpenWindow();
        }
        else PlantManager.Instance.CloseWindow();
    }

    public bool Manage(Field field)
    {
        if (!field || IsManaging) return false;
        CurrentField = field;
        IsManaging = true;
        OpenWindow();
        return true;
    }

    public void CancelManage()
    {
        IsManaging = false;
        CloseWindow();
    }

    public void UpdateUI()
    {
        UI.space.text = CurrentField.Crops.Count + "/" + CurrentField.Data.spaceOccup;
        UI.fertility.text = CurrentField.Data.fertility.ToString();
        UI.humidity.text = CurrentField.Data.fertility.ToString();
        foreach (var ca in cropAgents)
            ca.UpdateInfo();
    }

    public void UpdateCropsArea()
    {
        if (!IsUIOpen) return;
        while (cropAgents.Count < CurrentField.Crops.Count)
        {
            CropAgent ca = ObjectPool.Get(UI.cropPrefab, UI.cropCellsParent).GetComponent<CropAgent>();
            cropAgents.Add(ca);
        }
        while (cropAgents.Count > CurrentField.Crops.Count)
        {
            cropAgents[cropAgents.Count - 1].Clear(true);
            cropAgents.RemoveAt(cropAgents.Count - 1);
        }
        for (int i = 0; i < CurrentField.Crops.Count; i++)
            cropAgents[i].Init(CurrentField.Crops[i]);
    }

    public void DestroyCurrentField()
    {
        if (CurrentField) CurrentField.AskDestroy();
    }

    public void DispatchWorker()
    {
        MessageManager.Instance.New("敬请期待");
    }

    public void Plant(Crop crop)
    {
        CropAgent ca = ObjectPool.Get(UI.cropPrefab, UI.cropCellsParent).GetComponent<CropAgent>();
        ca.Init(crop);
    }
}
