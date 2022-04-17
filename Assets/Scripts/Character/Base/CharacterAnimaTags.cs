using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "char anima tags", menuName = "Zetan Studio/角色系统/角色动画标签")]
public class CharacterAnimaTags : SingletonScriptableObject<CharacterAnimaTags>
{
    [SerializeField]
    private string idle = "Idle";
    public static string Idle => Instance.idle;

    [SerializeField]
    private string move = "Move";
    public static string Move => Instance.move;

    [SerializeField]
    private string dash = "Dash";
    public static string Dash => Instance.dash;

    [SerializeField]
    private string roll = "Roll";
    public static string Roll => Instance.roll;

    [SerializeField]
    private string getHurt = "GetHurt";
    public static string GetHurt => Instance.getHurt;

    [SerializeField]
    private string gather = "Gather";
    public static string Gather => Instance.gather;

    [SerializeField]
    private string dead = "Dead";
    public static string Dead => Instance.dead;

    [SerializeField]
    private string relive = "Relive";
    public static string Relive => Instance.relive;

    [SerializeField]
    private string attack = "Attack";
    public static string Attack => Instance.attack;
}