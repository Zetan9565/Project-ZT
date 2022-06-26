using UnityEngine;

public abstract class CharacterInformation : ScriptableObject
{
    [SerializeField]
    protected string _ID;
    public string ID => _ID;

    [SerializeField]
    protected string _name;
    public string Name => _name;

    [SerializeField]
    protected CharacterSMParams _SMParams;
    public CharacterSMParams SMParams => _SMParams;

    //[SerializeField]
    //protected RoleAttributeGroup attribute;
    //public RoleAttributeGroup Attribute => attribute;

    public int level;

    public virtual bool IsValid => !string.IsNullOrEmpty(_ID) && !string.IsNullOrEmpty(_name);
}
public enum CharacterSex
{
    Unknown,
    Male,
    Female,
}