using UnityEngine;

[CreateAssetMenu(fileName = "enemy race", menuName = "Zetan Studio/敌人/敌人种族", order = 1)]
public class EnemyRace : ScriptableObject
{
    [SerializeField, Label("识别码")]
    private string _ID;
    public string ID
    {
        get
        {
            return _ID;
        }
    }

    [SerializeField, Label("种群名")]
    private string _name;
    public string Name
    {
        get
        {
            return _name;
        }
    }

    public override string ToString()
    {
        return !string.IsNullOrEmpty(_name) ? _name : base.ToString();
    }
}