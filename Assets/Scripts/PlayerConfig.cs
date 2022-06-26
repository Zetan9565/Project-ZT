using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerConfig : SingletonScriptableObject<PlayerConfig>
{
    [field: SerializeField, Header("背包相关")]
    public string BackpackName { get; private set; } = "背包";
    [field: SerializeField]
    public string WarehouseName { get; private set; } = "仓库";
    [field: SerializeField]
    public bool IgnoreLock { get; private set; } = false;
    [field: SerializeField]
    public int DefaultSpaceLimit { get; private set; } = 30;
    [field: SerializeField]
    public int MaxSpaceLimit { get; private set; } = 0;
    [field: SerializeField, Tooltip("不大于0表示不限制负重")]
    public float DefaultWeightLimit { get; private set; } = 100.0f;
    [field: SerializeField]
    public float MaxWeightLimit { get; private set; } = 0;
}