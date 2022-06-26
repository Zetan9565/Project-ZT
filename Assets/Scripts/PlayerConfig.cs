using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerConfig : SingletonScriptableObject<PlayerConfig>
{
    [field: SerializeField, Header("�������")]
    public string BackpackName { get; private set; } = "����";
    [field: SerializeField]
    public string WarehouseName { get; private set; } = "�ֿ�";
    [field: SerializeField]
    public bool IgnoreLock { get; private set; } = false;
    [field: SerializeField]
    public int DefaultSpaceLimit { get; private set; } = 30;
    [field: SerializeField]
    public int MaxSpaceLimit { get; private set; } = 0;
    [field: SerializeField, Tooltip("������0��ʾ�����Ƹ���")]
    public float DefaultWeightLimit { get; private set; } = 100.0f;
    [field: SerializeField]
    public float MaxWeightLimit { get; private set; } = 0;
}