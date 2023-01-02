using System;
using UnityEngine;
using ZetanStudio;

[Serializable]
public class CharacterData
{
    protected CharacterInformation info;

    public virtual CharacterInformation GetInfo()
    {
        return info;
    }
    public virtual T GetInfo<T>() where T : CharacterInformation
    {
        return info as T;
    }

    public CharacterStates mainState;
    public dynamic subState;
    public bool superArmor;
    public bool combat;
    public bool IsDead => mainState == CharacterStates.Abnormal && subState == CharacterAbnormalStates.Dead;

    public bool CanRoll
    {
        get
        {
            return mainState == CharacterStates.Normal;
        }
    }

    public bool CanFlash
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
            return mainState == CharacterStates.Abnormal;
        }
    }

    public Character entity;
    public string currentScene;

    public virtual Vector3 currentPosition => entity ? entity.Position : Vector3.zero;

    public CharacterData(CharacterInformation info)
    {
        this.info = info;
        currentScene = Utility.GetActiveScene().name;
    }

    public static implicit operator bool(CharacterData obj)
    {
        return obj != null;
    }
}

public enum CharacterStates
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

public enum CharacterNormalStates
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

public enum CharacterAbnormalStates
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

public enum CharacterGatherStates
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

public enum CharacterBusyStates
{
    [InspectorName("受伤")]
    GetHurt,
    [InspectorName("闪现")]
    Flash,
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
    [InspectorName("采集")]
    Gathering,
    [InspectorName("操作UI")]
    UI,
}

public enum CharacterAttackStates
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