using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "building info", menuName = "ZetanStudio/建筑物信息")]
public class BuildingInformation : ScriptableObject
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

    [SerializeField, NonReorderable]
    private List<MaterialInfo> materials = new List<MaterialInfo>();
    public virtual List<MaterialInfo> Materials
    {
        get
        {
            return materials;
        }
    }
}