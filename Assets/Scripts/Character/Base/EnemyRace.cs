using UnityEngine;

[CreateAssetMenu(fileName = "enemy race", menuName = "ZetanStudio/角色/敌人种族")]
public class EnemyRace : ScriptableObject
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("识别码")]
#endif
    private string _ID;
    public string ID
    {
        get
        {
            return _ID;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("种群名")]
#endif
    private string _name;
    public new string name
    {
        get
        {
            return _name;
        }
    }
}