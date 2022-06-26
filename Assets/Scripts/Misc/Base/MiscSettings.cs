using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "misc settings", menuName = "Zetan Studio/杂项设置")]
public class MiscSettings : SingletonScriptableObject<MiscSettings>
{
    [SerializeField]
    private string coinName = "铜币";

    [SerializeField]
    private float lootInvaildDistance;
    public float LootInvaildDistance => lootInvaildDistance;

    public string CoinName => ZetanStudio.LM.Tr(GetType().Name, coinName);

    [field: Header("任务相关"), SerializeField, SpriteSelector]
    public Sprite QuestIcon { get; private set; }
    [field: SerializeField]
    public QuestFlag QuestFlagsPrefab { get; private set; }

    [field: Header("关键字颜色"), SerializeField]
    public List<Color> KeywordColors { get; private set; } = new List<Color>()
    {
        Color.cyan,
        Color.yellow,
        Color.red
    };

    [field: SerializeField]
    public LootAgent DefaultLootPrefab { get; private set; }

    [field: SerializeField]
    public StructureFlag StructureFlagPrefab { get; private set; }
}