using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "building info", menuName = "ZetanStudio/建筑物信息")]
public class BuildingInformation : ScriptableObject
{
    [SerializeField]
    private string _IDPrefix;
    public string IDPrefix => _IDPrefix;

    [SerializeField]
    private string _name;
    public new string name => _name;

    [SerializeField]
    private string description;
    public string Description => description;

    [SerializeField]
    private bool manageable;
    public bool Manageable => manageable;

    [SerializeField]
    private string manageBtnName = "管理";
    public string ManageBtnName => manageBtnName;

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
    public Building Prefab => prefab;

    [SerializeField]
    private BuildingPreview preview;
    public BuildingPreview Preview => preview;

    [SerializeField]
    private List<MaterialInfo> materials = new List<MaterialInfo>();
    public virtual List<MaterialInfo> Materials => materials;
    [SerializeField]
    private List<Object> addendas = new List<Object>();
    public virtual List<Object> Addendas => addendas;
}