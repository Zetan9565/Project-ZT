using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "structure info", menuName = "Zetan Studio/设施信息")]
public class StructureInformation : ScriptableObject
{
    [SerializeField, ID]
    private string _ID;
    public string ID => _ID;

    [SerializeField, SpriteSelector]
    private Sprite icon;
    public Sprite Icon => icon;

    [SerializeField]
    private string _name;
    public string Name => _name;

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
    private Structure2D prefab;
    public Structure2D Prefab => prefab;

    [SerializeField]
    private StructurePreview2D preview;
    public StructurePreview2D Preview => preview;

    [SerializeField]
    private List<MaterialInfo> materials = new List<MaterialInfo>();
    public virtual List<MaterialInfo> Materials => materials;
    [SerializeField]
    private List<Object> addendas = new List<Object>();
    public virtual List<Object> Addendas => addendas;

    [SerializeField]
    private bool autoBuild;
    public bool AutoBuild => autoBuild;

    [SerializeField]
    private List<StructureStage> stages = new List<StructureStage>() { new StructureStage() };
    public List<StructureStage> Stages => stages;

    [SerializeField, Label("配方")]
    private Formulation formulation;
}

[System.Serializable]
public class StructureStage
{
    [SerializeField]
    private List<MaterialInfo> materials = new List<MaterialInfo>();
    public List<MaterialInfo> Materials => materials;

    [SerializeField]
    private Sprite graph;
    public Sprite Graph => graph;

    [SerializeField]
    private float buildTime = 10.0f;
    public float BuildTime => buildTime;

    [SerializeField, Range(0, 1)]
    private float destroyReturnRate = 0.5f;
    public float DestroyReturnRate => destroyReturnRate;

    [SerializeField]
    private Formulation formulation;
    public Formulation Formulation => formulation;
}