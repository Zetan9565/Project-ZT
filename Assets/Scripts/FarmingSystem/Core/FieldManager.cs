using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Zetan Studio/管理器/农田管理器")]
public class FieldManager : WindowHandler<FieldUI, FieldManager>
{
    public Field CurrentField { get; private set; }

    private readonly List<CropAgent> cropAgents = new List<CropAgent>();

    private readonly List<FieldData> fields = new List<FieldData>();

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
        if (!PlayerManager.Instance.CheckIsNormalWithAlert())
            return false;
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
        UI.space.text = CurrentField.Crops.Count + "/" + CurrentField.FData.spaceOccup;
        UI.fertility.text = CurrentField.FData.fertility.ToString();
        UI.humidity.text = CurrentField.FData.fertility.ToString();
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
        if (CurrentField) BuildingManager.Instance.DestroyBuilding(CurrentField.Data);
    }

    public void DispatchWorker()
    {
        MessageManager.Instance.New("敬请期待");
    }

    public void Reclaim(FieldData field)
    {
        lock (fields)
            fields.Add(field);
    }

    public void Plant(Crop crop)
    {
        CropAgent ca = ObjectPool.Get(UI.cropPrefab, UI.cropCellsParent).GetComponent<CropAgent>();
        ca.Init(crop);
    }

    public void Remove(Crop crop)
    {
        if (!crop) return;
        crop.Parent.Remove(crop);
        if (crop.UI) crop.UI.Clear(true);
        cropAgents.Remove(crop.UI);
    }

    public void Init()
    {
        fields.Clear();
        TimeManager.Instance.OnTimePassed -= TimePass;
        TimeManager.Instance.OnTimePassed += TimePass;
    }

    private void TimePass(decimal realTime)
    {
        using var fieldEnum = fields.GetEnumerator();
        while (fieldEnum.MoveNext())
            fieldEnum.Current.TimePass((float)realTime);
    }
}
