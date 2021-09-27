using System.Collections;
using UnityEngine;

public class PlayerData : CharacterData
{
    public PlayerInformation Info => GetInfo<PlayerInformation>();

    public PlayerData(CharacterInformation info) : base(info)
    {
    }
}