using UnityEngine;

[CreateAssetMenu(fileName = "character info", menuName = "Zetan Studio/角色/角色信息")]
public class CharacterInformation : ScriptableObject
{
    [SerializeField]
    protected string _ID;
    public string ID => _ID;

    [SerializeField]
    protected string _Name;
    public new string name => _Name;

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("未知", "男", "女")]
#endif
    protected CharacterSex sex;
    public CharacterSex Sex => sex;

    public virtual bool IsValid => !string.IsNullOrEmpty(_ID) && !string.IsNullOrEmpty(_Name);

    public static string GetSexString(CharacterSex sex)
    {
        switch (sex)
        {
            case CharacterSex.Male:
                return "男性";
            case CharacterSex.Female:
                return "女性";
            case CharacterSex.Unknown:
            default:
                return "未知";
        }
    }
}
public enum CharacterSex
{
    Unknown,
    Male,
    Female,
}