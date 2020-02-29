using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("ZetanStudio/管理器/种植管理器")]
public class PlantManager : SingletonMonoBehaviour<PlantManager>, IWindowHandler
{
    [SerializeField]
    private PlantUI UI;

    public List<SeedAgent> SeedAgents { get; private set; } = new List<SeedAgent>();

    public Field CurrentField { get; private set; }

    public bool PlantAble { get; private set; }

    public bool IsUIOpen { get; private set; }

    public bool IsPausing { get; private set; }

    public Canvas CanvasToSort => UI && UI.gameObject ? UI.windowCanvas : null;

    public void Init(Field field)
    {
        if (!UI || !UI.gameObject) return;
        CurrentField = field;
        var seeds = BackpackManager.Instance.Seeds.Select(x => x.item).ToList();
        while (seeds.Count > SeedAgents.Count)
        {
            SeedAgent sa = ObjectPool.Instance.Get(UI.seedCellPrefab, UI.seedCellsParent).GetComponent<SeedAgent>();
            SeedAgents.Add(sa);
        }
        while (seeds.Count < SeedAgents.Count)
        {
            SeedAgents[SeedAgents.Count - 1].Clear(true);
            SeedAgents.RemoveAt(SeedAgents.Count - 1);
        }
        for (int i = 0; i < seeds.Count; i++)
            SeedAgents[i].Init(seeds[i] as SeedItem);
        HideDescription();
    }

    public void Search()
    {
        if (!UI || !UI.gameObject) return;
        var name = UI.searchInput.text;
        if (string.IsNullOrEmpty(name)) ShowAll();
        else foreach (var sa in SeedAgents)
                if (!sa.name.Contains(name))
                    ZetanUtility.SetActive(sa.gameObject, false);
    }

    private int currentPage;
    public void SetPage(int index)
    {
        if (!UI || !UI.gameObject || index < 0) return;
        currentPage = index;
        switch (index)
        {
            case 1: ShowVegetables(); break;
            default: ShowAll(); break;
        }
        HideDescription();
    }

    private void ShowAll()
    {
        foreach (var sa in SeedAgents)
            ZetanUtility.SetActive(sa.gameObject, true);
    }

    private void ShowVegetables()
    {

    }

    private void ShowFruit()
    {

    }

    private void ShowTree()
    {

    }

    public void OpenWindow()
    {
        if (!CurrentField) return;
        if (!UI || !UI.gameObject) return;
        if (IsUIOpen) return;
        if (IsPausing) return;
        UI.window.alpha = 1;
        UI.window.blocksRaycasts = true;
        IsUIOpen = true;
        WindowsManager.Instance.Push(this);
        UIManager.Instance.EnableJoyStick(false);
        UIManager.Instance.EnableInteractive(false);
        UI.pageSelector.SetValueWithoutNotify(0);
        SetPage(0);
    }

    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing) return;
        UI.window.alpha = 0;
        UI.window.blocksRaycasts = false;
        IsUIOpen = false;
        WindowsManager.Instance.Remove(this);
        UI.searchInput.text = string.Empty;
        HideDescription();
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

    public void ShowDescription(SeedItem seed)
    {

    }

    public void HideDescription()
    {

    }
}
