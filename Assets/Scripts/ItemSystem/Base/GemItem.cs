﻿using UnityEngine;

[CreateAssetMenu(fileName = "gemstone", menuName = "ZetanStudio/道具/宝石")]
public class GemItem : ItemBase
{
    [SerializeField]
    private PowerUp powerup = new PowerUp();
    public PowerUp Powerup
    {
        get
        {
            return powerup;
        }
    }

    public GemItem()
    {
        itemType = ItemType.Gemstone;
    }
}