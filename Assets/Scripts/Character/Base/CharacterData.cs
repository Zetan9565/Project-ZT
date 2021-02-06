using System.Collections;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public CharacterInformation info;

    public CharacterState currentState;
    public bool superArmor;
    public bool combat;
    public bool IsDead => currentState == CharacterState.Dead;

    public bool CanRoll
    {
        get
        {
            switch (currentState)
            {
                case CharacterState.Idle:
                case CharacterState.Walk:
                case CharacterState.Run:
                    return true;
                default:
                    return false;
            }
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
            switch (currentState)
            {
                case CharacterState.Idle:
                case CharacterState.Walk:
                case CharacterState.Run:
                case CharacterState.Swim:
                    return true;
                default:
                    return false;
            }
        }
    }

    public string currentScene;
    public Vector3 currentPosition;

    public CharacterData(CharacterInformation info)
    {
        this.info = info;
    }

    public static implicit operator bool(CharacterData self)
    {
        return self != null;
    }
}

public enum CharacterState
{
    Dying = -2,
    Dead = -1,
    Idle,
    Walk,
    Run,
    Swim,
    Roll,
    Dash,
    Notice,
    Unsheathe,//拔、收刀
    Unsheathe_Move,//边走边拔、收刀
    Attak,
    Cast,
    Eat,
    Sit,
    Rely,
    Stun = 61,
    Fall = 62,
    Float = 63,
    Knockback = 64,
    Gather_Hand = 51,
    Gather_Axe = 52,
    Gather_Chan = 53,
    Gather_Gao = 55,
    Gather_Dao = 56,
}