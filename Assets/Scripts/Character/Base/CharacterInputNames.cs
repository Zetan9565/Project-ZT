using UnityEngine;

[CreateAssetMenu(fileName = "char input names", menuName = "Zetan Studio/角色/状态机/角色控制输入名称")]
public class CharacterInputNames : SingletonScriptableObject<CharacterInputNames>
{
    [field: SerializeField]
    public string Move { get; private set; } = "moveInput";
    [field:SerializeField]
    public string Direction { get; private set; } = "directionInput";
    [field: SerializeField]
    public string Roll { get; private set; } = "rollInput";
    [field: SerializeField]
    public string Flash { get; private set; } = "flashInput";
}