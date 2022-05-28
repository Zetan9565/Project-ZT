using UnityEngine;

[CreateAssetMenu(fileName = "enemy info", menuName = "Zetan Studio/敌人/敌人信息", order = 0)]
public class EnemyInformation : CharacterInformation
{
    [SerializeField]
    private EnemyRace race;
    public EnemyRace Race => race;

    [SerializeField, ObjectSelector(memberAsTooltip: "GetDropInfoString", displayAdd: true)]
    private ProductInformation dropItems;
    public ProductInformation DropItems
    {
        get
        {
            return dropItems;
        }
    }

    [SerializeField]
    private GameObject lootPrefab;
    public GameObject LootPrefab
    {
        get
        {
            return lootPrefab;
        }
    }
}
