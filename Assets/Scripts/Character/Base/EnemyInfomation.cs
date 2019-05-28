﻿using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "enemy info", menuName = "ZetanStudio/角色/敌人信息")]
public class EnemyInfomation : CharacterInfomation
{
    [SerializeField]
    private EnemyRace race;
    public EnemyRace Race
    {
        get
        {
            return race;
        }
    }

    [SerializeField]
    private List<DropItemInfo> dropItems = new List<DropItemInfo>();
    public List<DropItemInfo> DropItems
    {
        get
        {
            return dropItems;
        }
    }

}