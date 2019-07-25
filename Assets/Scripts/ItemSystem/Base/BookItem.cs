using UnityEngine;

[CreateAssetMenu(fileName = "book", menuName = "ZetanStudio/道具/书籍")]
public class BookItem : ItemBase
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("技能书", "建造图纸", "制作指南")]
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
    Skill,
    Building,
    Making
}