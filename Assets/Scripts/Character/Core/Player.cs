using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Player : Character
{
    [SerializeReference, ReadOnly]
    protected PlayerData data;

    public override CharacterData GetData()
    {
        return data;
    }

    public override void SetData(CharacterData value)
    {
        data = (PlayerData)value;
    }
}