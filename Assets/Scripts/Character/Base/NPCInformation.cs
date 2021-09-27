using UnityEngine;

[CreateAssetMenu(fileName = "npc info", menuName = "Zetan Studio/角色/NPC信息")]
public class NPCInformation : CharacterInformation
{
    [SerializeField, EnumMemberNames("未知", "男", "女")]
    protected CharacterSex sex;
    public CharacterSex Sex => sex;

    [SerializeField]
    protected bool enable;
    public bool Enable => enable;

    [SerializeField]
    protected string scene;
    public string Scene => scene;

    [SerializeField]
    protected Vector3 position;
    public Vector3 Position => position;

    [SerializeField]
    protected GameObject prefab;
    public GameObject Prefab => prefab;

    public override bool IsValid => base.IsValid && !enable || enable && !string.IsNullOrEmpty(scene) && prefab;

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