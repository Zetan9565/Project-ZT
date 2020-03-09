using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("ZetanStudio/管理器/农田管理器")]
public class FieldManager : WindowHandler<FieldUI, FieldManager>
{
    public Field CurrentField { get; private set; }

    private readonly List<CropAgent> cropAgents = new List<CropAgent>();

    public bool ManageAble { get; private set; }

    public override void OpenWindow()
    {
        if (!CurrentField) return;
        base.OpenWindow();
        if (!IsUIOpen) return;
        UpdateCropsArea();
        UpdateUI();
        UIManager.Instance.EnableJoyStick(false);
        UIManager.Instance.EnableInteract(false);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        ManageAble = false;
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

    public void CanManage(Field field)
    {
        if (!field) return;
        CurrentField = field;
        ManageAble = true;
        UIManager.Instance.EnableInteract(true, field.name);
    }

    public void CannotManage()
    {
        ManageAble = false;
        CurrentField = null;
        CloseWindow();
        UIManager.Instance.EnableInteract(false);
    }

    public void UpdateUI()
    {
        UI.space.text = CurrentField.Crops.Count + "/" + CurrentField.space;
        UI.fertility.text = CurrentField.fertility.ToString();
        UI.humidity.text = CurrentField.fertility.ToString();
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
}
