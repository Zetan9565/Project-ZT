using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "building info", menuName = "ZetanStudio/建筑/建筑物")]
public class BuildingInfo : ScriptableObject
{
    [SerializeField]
    private string _IDStarter;
    public string IDStarter
    {
        get
        {
            return _IDStarter;
        }
    }

    [SerializeField]
    private new string name;
    public string Name
    {
        get
        {
            return name;
        }
    }

    [SerializeField]
    private string description;
    public string Description
    {
        get
        {
            return description;
        }
    }

    [SerializeField]
    private float buildTime = 10.0f;
    public float BuildTime
    {
        get
        {
            if (buildTime < 0) buildTime = 0;
            return buildTime;
        }
    }

    [SerializeField]
    private Building prefab;
    public Building Prefab
    {
        get
        {
            return prefab;
        }
    }

    [SerializeField]
    private BuildingPreview preview;
    public BuildingPreview Preview
    {
        get
        {
            return preview;
        }
    }

    [SerializeField]
    private List<MatertialInfo> materials = new List<MatertialInfo>();
    public virtual List<MatertialInfo> Materials
    {
        get
        {
            return materials;
        }
    }


    public bool CheckMaterialsEnough(Backpack backpack, ref List<string> info)
    {
        var processInfo = materials.GetEnumerator();
        bool result = true;
        if (info == null) info = new List<string>();
        while (processInfo.MoveNext())
        {
            info.Add(string.Format("{0}\t[{1}/{2}]", processInfo.Current.ItemName, backpack.GetItemAmount(processInfo.Current.Item), processInfo.Current.Amount));
            if (backpack.GetItemAmount(processInfo.Current.Item) < processInfo.Current.Amount)
                result &= false;
        }
        return result;
    }
    public bool CheckMaterialsEnough(Backpack backpack)
    {
        var processInfo = materials.GetEnumerator();
        while (processInfo.MoveNext())
        {
            if (backpack.GetItemAmount(processInfo.Current.Item) < processInfo.Current.Amount)
                return false;
        }
        return true;
    }
}