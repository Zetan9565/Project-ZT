﻿using UnityEngine;

[CreateAssetMenu(fileName = "book", menuName = "ZetanStudio/道具/书籍")]
public class BookItem : ItemBase
{
    [SerializeField]
    private BookType bookType;
    public BookType BookType
    {
        get
        {
            return bookType;
        }
    }

    [SerializeField]
    private BuildingInformation buildingToLearn;
    public BuildingInformation BuildingToLearn
    {
        get
        {
            return buildingToLearn;
        }
    }

    [SerializeField]
    private ItemBase itemToLearn;
    public ItemBase ItemToLearn
    {
        get
        {
            return itemToLearn;
        }
    }


    public BookItem()
    {
        itemType = ItemType.Book;
    }
}

public enum BookType
{
    [InspectorName("技能书")]
    Skill,

    [InspectorName("建造图纸")]
    Building,

    [InspectorName("制作指南")]
    Making
}