using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.CharacterSystem;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Module;
using ZetanStudio;
using ZetanStudio.ItemSystem.UI;
using ZetanStudio.UI;
using ZetanStudio.InventorySystem;

public class ItemInfoDisplayer : MonoBehaviour
{
    [SerializeField]
    private Image icon;
    public Image Icon => icon;
    [SerializeField]
    private GameObject contrastMark;
    public GameObject ContrastMark => contrastMark;
    [SerializeField]
    private Text levelText;
    public Text LevelText => levelText;
    [SerializeField]
    private Text nameText;
    public Text NameText => nameText;
    [SerializeField]
    private Text typeText;
    public Text TypeText => typeText;

    [SerializeField]
    private Text priceTitle;
    public Text PriceTitle => priceTitle;
    [SerializeField]
    private Text priceText;
    public Text PriceText => priceText;
    [SerializeField]
    private Text weightTitle;
    public Text WeightTitle => weightTitle;
    [SerializeField]
    private Text weightText;
    public Text WeightText => weightTitle;

    [SerializeField]
    private Transform contentParent;
    [SerializeField]
    private RectTransform mainAttrParent;

    [SerializeField]
    private DurabilityAgent durability;

    [SerializeField]
    private Text titlePrefab;
    [SerializeField]
    private Text contentPrefab;
    [SerializeField]
    private TitledContent titledContentPrefab;
    [SerializeField]
    private LayoutElement separatorPrefab;
    [SerializeField]
    private ItemPropertyAgent attributePrefab;
    [SerializeField]
    private GemAgent gemPrefab;
    [SerializeField]
    private LayoutElement customizedContainerPrefab;
    [SerializeField]
    private RectTransform[] customizedPrefabs;

    [SerializeField]
    private Text debugIDText;

    private readonly List<Text> titles = new List<Text>();
    private readonly List<Text> contents = new List<Text>();
    private readonly List<TitledContent> titledContents = new List<TitledContent>();
    private readonly List<ItemPropertyAgent> attributes = new List<ItemPropertyAgent>();
    private readonly List<GemAgent> gems = new List<GemAgent>();
    private readonly List<LayoutElement> separators = new List<LayoutElement>();
    private readonly List<LayoutElement> customizedContainers = new List<LayoutElement>();
    private readonly Dictionary<int, List<RectTransform>> customizeds = new Dictionary<int, List<RectTransform>>();

    private SimplePool<Text> titleCache;
    private SimplePool<Text> contentCache;
    private SimplePool<TitledContent> titledContentCache;
    private SimplePool<ItemPropertyAgent> attrCache;
    private SimplePool<GemAgent> gemCache;
    private SimplePool<LayoutElement> separCache;
    private SimplePool<LayoutElement> customizedContainerCache;
    private Dictionary<int, SimplePool<RectTransform>> customizedCache;

    private int elementsCount;
    public int Count => elementsCount;

    private ItemData item;
    public ItemData Item => item;

    private ItemWindow window;
    public ItemWindow Window => window;

    public void SetWindow(ItemWindow window)
    {
        this.window = window;
    }

    public void ShowItem(ItemData item)
    {
        if (!item || !item.Model)
        {
            Hide();
            return;
        }
        if (this.item == item) return;
        Clear();
        this.item = item;
        icon.overrideSprite = item.Icon;
        Utility.SetActive(contrastMark, item != window.Item);
        nameText.text = item.Name;
        nameText.color = item.Quality.Color;
        typeText.text = item.Type.Name;
        if (item.TryGetModule<SellableModule>(out var sellAble))
        {
            Utility.SetActive(priceTitle, true);
            priceText.text = sellAble.Price + Tr(MiscSettings.Instance.CoinName);
        }
        else Utility.SetActive(priceTitle, false);
        Utility.SetActive(weightTitle, item.Weight > 0);
        weightText.text = item.Weight.ToString("F2");
        if (item.TryGetModuleData<AttributeData>(out var attribute))
        {
            foreach (var attr in attribute.properties)
            {
                AddAttribute(attr);
            }
        }
        if (item.TryGetModuleData<AffixData>(out var affix))
        {
            if (affix.affixes.Count > 0)
            {
                AddTitle(Tr("附加效果:"));
                foreach (var attr in affix.affixes)
                {
                    AddAffix(attr);
                }
            }
        }
        levelText.text = Item.TryGetModuleData<EnhancementData>(out var enhancement) && enhancement.level > 0 ? (!enhancement.IsMax ? $"+{enhancement.level}" : "MAX") : string.Empty;
        AddTitle(Tr("描述:"));
        AddContent(item.Description);
        foreach (var module in item.Modules)
        {
            if (module is IItemWindowModifier modifier)
                modifier.ModifyItemWindow(this);
        }
        Utility.SetActive(durability, item.GetModule<DurabilityModule>());
#if DEBUG
        Utility.SetActive(debugIDText, true);
        debugIDText.text = item.ID;
#else
        ZetanUtility.SetActive(debugIDText, false);
#endif
        Show();
    }

    public void AddTitledContent(string title, string content, int? index = null)
    {
        TitledContent titledContent = titledContentCache.Get(contentParent);
        titledContent.Init(title, content);
        titledContent.transform.SetSiblingIndex(index ?? elementsCount);
        titledContents.Add(titledContent);
        elementsCount++;
    }

    public void AddTitle(string content, int? index = null)
    {
        Text title = titleCache.Get(contentParent);
        title.text = content;
        title.transform.SetSiblingIndex(index ?? elementsCount);
        titles.Add(title);
        elementsCount++;
    }

    public void AddContent(string content, int? index = null)
    {
        Text cont = contentCache.Get(contentParent);
        cont.text = content;
        cont.transform.SetSiblingIndex(index ?? elementsCount);
        contents.Add(cont);
        elementsCount++;
    }

    public void AddAttribute(ItemProperty left, int? index = null)
    {
        ItemPropertyAgent attr = attrCache.Get(mainAttrParent);
        attr.Init(left);
        attr.transform.SetSiblingIndex(index ?? mainAttrParent.childCount - 1);
        attributes.Add(attr);
    }
    public void AddAffix(ItemProperty left, int? index = null)
    {
        ItemPropertyAgent attr = attrCache.Get(contentParent);
        attr.Init(left);
        attr.transform.SetSiblingIndex(index ?? elementsCount);
        attributes.Add(attr);
        elementsCount++;
    }

    public void AddGem(Item gem, int? index = null)
    {
        GemAgent agent = gemCache.Get(contentParent);
        agent.Init(gem);
        agent.transform.SetSiblingIndex(index ?? elementsCount);
        gems.Add(agent);
        elementsCount++;
    }

    public void AddSeparator(float? height = null, int? index = null)
    {
        LayoutElement separ = separCache.Get(contentParent);
        separ.minHeight = height ?? separatorPrefab.minHeight;
        separ.transform.SetSiblingIndex(index ?? elementsCount);
        separators.Add(separ);
        elementsCount++;
    }

    public void AddElement(int prefabIndex, Action<RectTransform> modifier, int? index = null)
    {
        if (prefabIndex < 0 || prefabIndex > customizedPrefabs.Length) return;
        RectTransform element = customizedCache[prefabIndex].Get(contentParent);
        modifier?.Invoke(element);
        element.SetSiblingIndex(index ?? elementsCount);
        if (!customizeds.TryGetValue(prefabIndex, out var list))
        {
            list = new List<RectTransform>() { element };
            customizeds.Add(prefabIndex, list);
        }
        else list.Add(element);
        elementsCount++;
    }
    public void AddElement(RectTransform element, float? height = null, int? index = null)
    {
        LayoutElement layout = customizedContainerCache.Get(contentParent);
        element.SetParent(layout.transform);
        layout.minHeight = height ?? customizedContainerPrefab.minHeight;
        layout.transform.SetSiblingIndex(index ?? elementsCount);
        customizedContainers.Add(layout);
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
        foreach (var content in titledContents)
        {
            content.Clear();
            titledContentCache.Put(content);
        }
        titledContents.Clear();
        foreach (ItemPropertyAgent attribute in attributes)
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
        foreach (var gem in gems)
        {
            gem.Clear();
            gemCache.Put(gem);
        }
        gems.Clear();
        foreach (var container in customizedContainers)
        {
            foreach (Transform child in container.transform)
            {
                ObjectPool.Put(child);
            }
            customizedContainerCache.Put(container);
        }
        customizedContainers.Clear();
        foreach (var kvp in customizeds)
        {
            foreach (RectTransform element in kvp.Value)
            {
                customizedCache[kvp.Key].Put(element);
            }
            kvp.Value.Clear();
        }
        customizeds.Clear();
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
        Utility.SetActive(gameObject, true);
    }
    public void Hide()
    {
        Clear();
        Utility.SetActive(gameObject, false);
    }

    private void Awake()
    {
        titleCache = new SimplePool<Text>(titlePrefab);
        contentCache = new SimplePool<Text>(contentPrefab);
        titledContentCache = new SimplePool<TitledContent>(titledContentPrefab);
        separCache = new SimplePool<LayoutElement>(separatorPrefab);
        attrCache = new SimplePool<ItemPropertyAgent>(attributePrefab);
        gemCache = new SimplePool<GemAgent>(gemPrefab);
        customizedContainerCache = new SimplePool<LayoutElement>(customizedContainerPrefab);
        customizedCache = new Dictionary<int, SimplePool<RectTransform>>();
        for (int i = 0; i < customizedPrefabs.Length; i++)
        {
            customizedCache.Add(i, new SimplePool<RectTransform>(customizedPrefabs[i]));
        }
#if DEBUG
        debugIDText.GetComponent<Button>().onClick.AddListener(DebugGetter);
#endif
    }

    private string Tr(string text)
    {
        return LM.Tr(GetType().Name, text);
    }

#if DEBUG
    private void DebugGetter()
    {
        if (item)
        {
            AmountWindow.StartInput(a => { if (item) BackpackManager.Instance.Get(item.Model, (int)a); }, 999, position: icon.transform.position);
        }
    }
#endif
}
public interface IItemWindowModifier
{
    public void ModifyItemWindow(ItemInfoDisplayer displayer);
}