using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoDisplayer : MonoBehaviour
{
    public Image icon;
    public Text nameText;
    public Text typeText;

    public Text priceTitle;
    public Text priceText;
    public Text weightText;

    public Transform contentParent;

    public Text titlePrefab;
    public Text contentPrefab;
    public RoleAttributeAgent attributePrefab;
    public GemstoneAgent gemPrefab;

    private List<Text> titles = new List<Text>();
    private List<Text> contents = new List<Text>();
    private List<RoleAttributeAgent> attributes = new List<RoleAttributeAgent>();

    private Stack<Text> titleCache = new Stack<Text>();
    private Stack<Text> contentCache = new Stack<Text>();
    private Stack<RoleAttributeAgent> attrCache = new Stack<RoleAttributeAgent>();

    private int lineCount;

    private ItemInfo info;

    public void ShowItemInfo(ItemInfo info)
    {
        if (this.info == info) return;
        Clear();
        this.info = info;
        icon.overrideSprite = info.item.Icon;
        nameText.text = info.ItemName;
        nameText.color = GameManager.QualityToColor(info.item.Quality);
        typeText.text = ItemBase.GetItemTypeString(info.item.ItemType);
        priceText.text = info.item.SellAble ? info.item.SellPrice + GameManager.CoinName : "不可出售";
        weightText.text = info.item.Weight.ToString("F2") + "WL";
        if (info.item.IsEquipment)
        {
            PushTitle("属性：");
            foreach (RoleAttribute attr in (info.item as EquipmentItem).Attribute.Attributes)
            {
                PushAttribute(attr);
            }
        }
        PushTitle("描述：");
        PushContent(info.item.Description);
        Show();
    }

    private void PushTitle(string content)
    {
        Text find;
        if (titleCache.Count < 1)
            find = ObjectPool.Get(titlePrefab, contentParent).GetComponent<Text>();
        else find = titleCache.Pop();
        ZetanUtility.SetActive(find.gameObject, true);
        find.text = content;
        find.transform.SetSiblingIndex(lineCount);
        titles.Add(find);
        lineCount++;
    }

    private void PushContent(string content)
    {
        Text find;
        if (contentCache.Count < 1)
            find = ObjectPool.Get(contentPrefab, contentParent).GetComponent<Text>();
        else find = contentCache.Pop();
        ZetanUtility.SetActive(find.gameObject, true);
        find.text = content;
        find.transform.SetSiblingIndex(lineCount);
        contents.Add(find);
        lineCount++;
    }

    private void PushAttribute(RoleAttribute left, RoleAttribute right = null)
    {
        RoleAttributeAgent find;
        if (attrCache.Count < 1)
            find = ObjectPool.Get(attributePrefab, contentParent).GetComponent<RoleAttributeAgent>();
        else find = attrCache.Pop();
        ZetanUtility.SetActive(find, true);
        find.Init(left, right);
        find.transform.SetSiblingIndex(lineCount);
        attributes.Add(find);
        lineCount++;
    }

    private void PushGem()
    {

    }

    public void Clear()
    {
        foreach (Text title in titles)
        {
            title.text = string.Empty;
            titleCache.Push(title);
            ZetanUtility.SetActive(title, false);
        }
        titles.Clear();
        foreach (Text content in contents)
        {
            content.text = string.Empty;
            contentCache.Push(content);
            ZetanUtility.SetActive(content, false);
        }
        contents.Clear();
        foreach (RoleAttributeAgent attribute in attributes)
        {
            attribute.Clear();
            attrCache.Push(attribute);
            ZetanUtility.SetActive(attribute, false);
        }
        attributes.Clear();
        lineCount = 0;
        info = null;
        icon.overrideSprite = null;
        nameText.text = string.Empty;
        typeText.text = string.Empty;
        priceText.text = string.Empty;
        weightText.text = string.Empty;
    }

    public void Show()
    {
        ZetanUtility.SetActive(gameObject, true);
    }
    public void Hide(bool clear = false)
    {
        if (clear)
            Clear();
        ZetanUtility.SetActive(gameObject, false);
    }
}