using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoDisplayer : MonoBehaviour
{
    [SerializeField]
    private Image icon;
    [SerializeField]
    private GameObject contrastMark;
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text typeText;

    [SerializeField]
    private Text priceTitle;
    [SerializeField]
    private Text priceText;
    [SerializeField]
    private Text weightText;

    [SerializeField]
    private Transform contentParent;

    [SerializeField]
    private DurabilityAgent durability;

    [SerializeField]
    private Text titlePrefab;
    [SerializeField]
    private Text contentPrefab;
    [SerializeField]
    private LayoutElement separatorPrefab;
    [SerializeField]
    private RoleAttributeAgent attributePrefab;
    [SerializeField]
    private GemAgent gemPrefab;

    [SerializeField]
    private Text debugIDText;

    private List<Text> titles = new List<Text>();
    private List<Text> contents = new List<Text>();
    private List<RoleAttributeAgent> attributes = new List<RoleAttributeAgent>();
    private List<GemAgent> gems = new List<GemAgent>();
    private List<LayoutElement> separators = new List<LayoutElement>();

    private SimplePool<Text> titleCache;
    private SimplePool<Text> contentCache;
    private SimplePool<RoleAttributeAgent> attrCache;
    private SimplePool<LayoutElement> separCache;

    private int elementsCount;

    private ItemData item;

    public void ShowItemInfo(ItemData item, bool isContrast = false)
    {
        if (!item || !item.Model_old)
        {
            Hide(true);
            return;
        }
        if (this.item == item) return;
        Clear();
        this.item = item;
        icon.overrideSprite = item.Model_old.Icon;
        ZetanUtility.SetActive(contrastMark, isContrast);
        nameText.text = item.Name;
        nameText.color = ItemUtility.QualityToColor(item.Model_old.Quality);
        typeText.text = ItemBase.GetItemTypeString(item.Model_old.ItemType);
        priceText.text = item.Model_old.SellAble ? item.Model_old.SellPrice + GameManager.CoinName : "不可出售";
        weightText.text = item.Model_old.Weight.ToString("F2") + "WL";
        if (item.Model_old.IsEquipment)
        {
            PushTitle("属性：");
            foreach (RoleAttribute attr in (item.Model_old as EquipmentItem).Attribute.Attributes)
            {
                PushAttribute(attr);
            }
        }
        PushTitle("描述：");
        PushContent(item.Model_old.Description);
        ZetanUtility.SetActive(durability, item.Model_old.IsEquipment);
#if true
        ZetanUtility.SetActive(debugIDText, true);
        debugIDText.text = item.ID;
#else
        ZetanUtility.SetActive(debugIDText, false);
#endif
        Show();
    }

    private void PushTitle(string content)
    {
        Text title = titleCache.Get(contentParent);
        title.text = content;
        title.transform.SetSiblingIndex(elementsCount);
        titles.Add(title);
        elementsCount++;
    }

    private void PushContent(string content)
    {
        Text cont = contentCache.Get(contentParent);
        cont.text = content;
        cont.transform.SetSiblingIndex(elementsCount);
        contents.Add(cont);
        elementsCount++;
    }

    private void PushAttribute(RoleAttribute left, RoleAttribute right = null)
    {
        RoleAttributeAgent attr = attrCache.Get(contentParent);
        attr.Init(left, right);
        attr.transform.SetSiblingIndex(elementsCount);
        attributes.Add(attr);
        elementsCount++;
    }

    private void PushGem(GemItem gem)
    {
        elementsCount++;
    }

    private void PushSeparator(float? height)
    {
        LayoutElement separ = separCache.Get(contentParent);
        if (height.HasValue) separ.minHeight = height.Value;
        separ.transform.SetSiblingIndex(elementsCount);
        separators.Add(separ);
        elementsCount++;
    }

    public void Clear()
    {
        foreach (Text title in titles)
        {
            title.text = string.Empty;
            titleCache.Put(title);
        }
        titles.Clear();
        foreach (Text content in contents)
        {
            content.text = string.Empty;
            contentCache.Put(content);
        }
        contents.Clear();
        foreach (RoleAttributeAgent attribute in attributes)
        {
            attribute.Clear();
            attrCache.Put(attribute);
        }
        attributes.Clear();
        foreach (var separ in separators)
        {
            separCache.Put(separ);
        }
        separators.Clear();
        elementsCount = 0;
        item = null;
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
        if (clear) Clear();
        ZetanUtility.SetActive(gameObject, false);
    }

    private void Awake()
    {
        titleCache = new SimplePool<Text>(titlePrefab);
        contentCache = new SimplePool<Text>(contentPrefab);
        separCache = new SimplePool<LayoutElement>(separatorPrefab);
        attrCache = new SimplePool<RoleAttributeAgent>(attributePrefab);
#if DEBUG
        debugIDText.GetComponent<Button>().onClick.AddListener(DebugGetter);
#endif
    }

#if DEBUG
    private void DebugGetter()
    {
        if (item)
        {
            AmountWindow.StartInput(a => { if (item) BackpackManager.Instance.GetItem(item.Model_old, (int)a); }, 999, position: icon.transform.position);
        }
    }
#endif
}