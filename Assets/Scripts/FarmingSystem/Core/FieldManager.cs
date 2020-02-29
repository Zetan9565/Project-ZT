﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("ZetanStudio/管理器/农田管理器")]
public class FieldManager : SingletonMonoBehaviour<FieldManager>, IWindowHandler
{
    [SerializeField]
    private FieldUI UI;

    public Field CurrentField { get; private set; }

    private readonly List<CropAgent> cropAgents = new List<CropAgent>();

    public bool IsUIOpen { get; private set; }

    public bool IsPausing { get; private set; }

    public Canvas CanvasToSort => UI ? UI.windowCanvas : null;

    public bool ManageAble { get; private set; }

    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing) return;
        UI.window.alpha = 0;
        UI.window.blocksRaycasts = false;
        IsUIOpen = false;
        WindowsManager.Instance.Remove(this);
    }

    public void OpenWindow()
    {
        if (!CurrentField) return;
        if (!UI || !UI.gameObject) return;
        if (IsUIOpen) return;
        if (IsPausing) return;
        UpdateCropsArea();
        UpdateUI();
        UI.window.alpha = 1;
        UI.window.blocksRaycasts = true;
        IsUIOpen = true;
        WindowsManager.Instance.Push(this);
        UIManager.Instance.EnableJoyStick(false);
        UIManager.Instance.EnableInteractive(false);
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

    public void PauseDisplay(bool pause)
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.window.alpha = 1;
            UI.window.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.window.alpha = 0;
            UI.window.blocksRaycasts = false;
        }
        IsPausing = pause;
    }

    public void CanManage(Field field)
    {
        if (!field) return;
        CurrentField = field;
        ManageAble = true;
        UIManager.Instance.EnableInteractive(true, field.name);
    }

    public void CannotManage()
    {
        ManageAble = false;
        CurrentField = null;
        CloseWindow();
        UIManager.Instance.EnableInteractive(false);
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
            CropAgent ca = ObjectPool.Instance.Get(UI.cropPrefab, UI.cropCellsParent).GetComponent<CropAgent>();
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
        if (!CurrentField) return;
        CurrentField.AskDestroy();
    }
}