using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "char anima param names", menuName = "Zetan Studio/角色系统/角色动画参数名称")]
public class CharacterAnimaParams : SingletonScriptableObject<CharacterAnimaParams>
{
    [SerializeField]
    private string directionX = "horizontal";
    public static string DirectionX => Instance.directionX;

    [SerializeField]
    private string directionY = "vertical";
    public static string DirectionY => Instance.directionY;

    [SerializeField]
    private string dash = "dash";
    public static string Dash => Instance.dash;

    [SerializeField]
    private string roll = "roll";
    public static string Roll => Instance.roll;

    [SerializeField]
    private string getHurt = "getHurt";
    public static string GetHurt => Instance.getHurt;

    [SerializeField]
    private string hurtDirX = "hurtDirX";
    public static string HurtDirX => Instance.hurtDirX;

    [SerializeField]
    private string hurtDirY = "hurtDirY";
    public static string HurtDirY => Instance.hurtDirY;

    [SerializeField]
    private string gather = "gather";
    public static string Gather => Instance.gather;

    [SerializeField]
    private string dead = "dead";
    public static string Dead => Instance.dead;

    [SerializeField]
    private string relive = "relive";
    public static string Relive => Instance.relive;

    [SerializeField]
    private string combat = "combat";
    public static string Combat => Instance.combat;

    [SerializeField]
    private string superArmor = "superArmor";
    public static string SuperArmor => Instance.superArmor;

    [SerializeField]
    private string state = "state";
    public static string State => Instance.state;

    [SerializeField]
    private string subState = "subState";
    public static string SubState => Instance.subState;

    [SerializeField]
    private string attack = "attack";
    public static string Attack => Instance.attack;

    [SerializeField]
    private string weapon = "weapon";
    public static string Weapon => Instance.weapon;

    [SerializeField]
    private string normalize = "normalize";
    public static string Normalize => Instance.normalize;

    [SerializeField]
    private string desiredSpeed = "desiredSpeed";
    public static string DesiredSpeed => Instance.desiredSpeed;

    [SerializeField]
    private string inputTimeout = "inputTimeout";
    public static string InputTimeout => Instance.inputTimeout;

    [SerializeField]
    private string interrupt = "interrupt";
    public static string Interrupt => Instance.interrupt;
}