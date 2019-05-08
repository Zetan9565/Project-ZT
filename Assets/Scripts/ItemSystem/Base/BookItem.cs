using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "book", menuName = "ZetanStudio/道具/书籍")]
public class BookItem : ItemBase
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("技能书", "建造图纸")]
#endif
    private BookType bookType;
    public BookType BookType
    {
        get
        {
            return bookType;
        }
    }

    [SerializeField]
    private BuildingInfo buildingInfo;
    public BuildingInfo BuildingInfo
    {
        get
        {
            return buildingInfo;
        }
    }

    public BookItem()
    {
        itemType = ItemType.Book;
    }
}

public enum BookType
{
    Skill,
    Building
}