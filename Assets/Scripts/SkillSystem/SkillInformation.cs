using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill info", menuName = "Zetan Studio/技能信息")]
public class SkillInformation : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID
    {
        get
        {
            return _ID;
        }
    }

    [SerializeField]
    private string _name;
    public string Name
    {
        get
        {
            return _name;
        }
    }

    [SerializeField]
    private string description;
    public string Description
    {
        get
        {
            return description;
        }
    }

    [SerializeField]
    private string animaTriggerName;
    public string AnimaTriggerName
    {
        get
        {
            return animaTriggerName;
        }
    }

    [SerializeField]
    private List<SkillAction> skillActions;//技能段数
    public List<SkillAction> SkillActions
    {
        get
        {
            return skillActions;
        }
    }
}

[System.Serializable]
public class SkillAction
{
    [SerializeField]
    private int damageCount;//伤害判定的次数
    public int DamageCount
    {
        get
        {
            return damageCount;
        }
    }

    [SerializeField]
    private float damageMultiple;//每次判定造成伤害的倍数
    public float DamageMultiple
    {
        get
        {
            return damageMultiple;
        }
    }

    [SerializeField]
    private int expendMP;//MP消耗
    public int ExpendMP
    {
        get
        {
            return expendMP;
        }
    }

    [SerializeField]
    private int expendMPIncrOnLV_UP;//MP消耗每级增量
    public int ExpendMPIncrOnLV_UP
    {
        get
        {
            return expendMPIncrOnLV_UP;
        }
    }

    [SerializeField]
    private int recoverHP;//每次攻击恢复的HP
    public int RecoverHP
    {
        get
        {
            return recoverHP;
        }
    }

    [SerializeField]
    private int recHPIncrOnLV_UP;//攻击恢复HP每级增量
    public int RecHPIncrOnLV_UP
    {
        get
        {
            return recHPIncrOnLV_UP;
        }
    }

    [SerializeField]
    private int recoverMP;//每次攻击恢复的MP
    public int RecoverMP
    {
        get
        {
            return recoverMP;
        }
    }

    [SerializeField]
    private int recMPIncrOnLV_UP;//攻击恢复MP每级增量
    public int RecMPIncrOnLV_UP
    {
        get
        {
            return recMPIncrOnLV_UP;
        }
    }

    [SerializeField, Range(0, 1)]
    private float inputListenBeginNrmlzTime = 0.5f;
    public float InputListenBeginNrmlzTime
    {
        get
        {
            return inputListenBeginNrmlzTime;
        }
    }

    [SerializeField, Range(0, 1)]
    private float inputTimeOutNrmlzTime = 0.95f;
    public float InputTimeOutNrmlzTime
    {
        get
        {
            return inputTimeOutNrmlzTime;
        }
    }

    public static implicit operator bool(SkillAction obj)
    {
        return obj != null;
    }
}