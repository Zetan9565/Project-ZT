using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CraftAgent : ListItem<CraftAgent, ZetanStudio.Item.Item>
{
    [SerializeField]
    private ItemSlotBase icon;

    [SerializeField]
    private Text nameText;

    private CraftWindow window;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (Data) window.ShowDescription(Data);
    }

    public override void Refresh()
    {
        icon.SetItem(Data);
        nameText.text = Data.Name;
    }

    public void SetWindow(CraftWindow window)
    {
        this.window = window;
    }

    public override void OnClear()
    {
        base.OnClear();
        nameText.text = string.Empty;
        Data = null;
        icon.Vacate();
    }
}
