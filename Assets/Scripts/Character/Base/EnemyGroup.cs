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
    public new string name => _name;

    [SerializeField]
    private List<EnemyInformation> enemies = new List<EnemyInformation>();
    public List<EnemyInformation> Enemies => enemies;

}