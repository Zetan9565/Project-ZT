using System;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public CharacterInformation info;

    public CharacterState mainState;
    public dynamic subState;
    public bool superArmor;
    public bool combat;
    public bool IsDead => mainState == CharacterState.Abnormal && subState == CharacterAbnormalState.Dead;

    public bool CanRoll
    {
        get
        {
            return mainState == CharacterState.Normal;
        }
    }

    public bool CanDash
    {
        get
        {
            return combat && CanRoll;
        }
    }

    public bool CanMove
    {
        get
        {
            return mainState == CharacterState.Abnormal;
        }
    }

    public Character entity;
    public string currentScene;

    public virtual Vector3 currentPosition => entity ? entity.Position : Vector3.zero;

    public CharacterData(CharacterInformation info)
    {
        this.info = info;
        currentScene = ZetanUtility.ActiveScene.name;
    }

    public static implicit operator bool(CharacterData self)
    {
        return self != null;
    }
}

public enum CharacterState
{
    [InspectorName("普通")]
    Normal,
    [InspectorName("异常")]
    Abnormal,
    [InspectorName("采集")]
    Gather,
    [InspectorName("攻击")]
    Attack,
    [InspectorName("忙碌")]
    Busy
}

public enum CharacterNormalState
{
    [InspectorName("待机")]
    Idle,
    [InspectorName("步行")]
    Walk,
    [InspectorName("疾跑")]
    Run,
    [InspectorName("游泳")]
    Swim,
}

public enum CharacterAbnormalState
{
    [InspectorName("死亡")]
    Dead,
    [InspectorName("眩晕")]
    Stun,
    [InspectorName("倒地")]
    Fall,
    [InspectorName("浮空")]
    Float,
    [InspectorName("击退")]
    Knockback,
}

public enum CharacterGatherState
{
    [InspectorName("手动")]
    Gather_Hand,
    [InspectorName("斧砍")]
    Gather_Axe,
    [InspectorName("铲挖")]
    Gather_Chan,
    [InspectorName("镐敲")]
    Gather_Gao,
    [InspectorName("捣碎")]
    Gather_Dao,
}

public enum CharacterBusyState
{
    [InspectorName("受伤")]
    GetHurt,
    [InspectorName("闪现")]
    Dash,
    [InspectorName("翻滚")]
    Roll,
    [InspectorName("格挡")]
    Parry,
    [InspectorName("拔刀")]
    Unsheathe,
    [InspectorName("吃食")]
    Eat,
    [InspectorName("坐着")]
    Sit,
    [InspectorName("垂钓")]
    Fishing,
    [InspectorName("谈话")]
    Talking,
    [InspectorName("操作UI")]
    UI,
}

public enum CharacterAttackState
{
    [InspectorName("招式1")]
    Action_1,
    [InspectorName("招式2")]
    Action_2,
    [InspectorName("招式3")]
    Action_3,
    [InspectorName("招式4")]
    Action_4,
}