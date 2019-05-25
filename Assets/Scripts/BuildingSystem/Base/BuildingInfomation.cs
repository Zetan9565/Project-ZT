using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "building info", menuName = "ZetanStudio/建筑/建筑物")]
public class BuildingInfomation : ScriptableObject
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


    public IEnumerable<string> GetMaterialsInfo(Backpack backpack)
    {
        List<string> info = new List<string>();
        using (var makingInfo = materials.GetEnumerator())
            while (makingInfo.MoveNext())
                info.Add(string.Format("{0}\t[{1}/{2}]", makingInfo.Current.ItemName, backpack.GetItemAmount(makingInfo.Current.Item), makingInfo.Current.Amount));
        return info.AsEnumerable();
    }
    public bool CheckMaterialsEnough(Backpack backpack)
    {
        var materialEnum = materials.GetEnumerator();
        while (materialEnum.MoveNext())
        {
            if (backpack.GetItemAmount(materialEnum.Current.Item) < materialEnum.Current.Amount)
                return false;
        }
        return true;
    }
}