using UnityEngine;

[CreateAssetMenu(fileName = "enemy race", menuName = "Zetan Studio/敌人/敌人种族", order = 1)]
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
    public string Name
    {
        get
        {
            return _name;
        }
    }
}