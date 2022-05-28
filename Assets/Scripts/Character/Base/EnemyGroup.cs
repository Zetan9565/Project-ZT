using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "enemy group", menuName = "Zetan Studio/敌人/敌人组合", order = 2)]
public class EnemyGroup : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField]
    private string _name;
    public string Name => _name;

    [SerializeField]
    private List<EnemyInformation> enemies = new List<EnemyInformation>();
    public List<EnemyInformation> Enemies => enemies;

    public bool Contains(string ID)
    {
        return enemies.Exists(x => x.ID == ID);
    }

    public override string ToString()
    {
        return !string.IsNullOrEmpty(_name) ? _name : base.ToString();
    }
}