using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CraftAgent : ListItem<CraftAgent, ZetanStudio.ItemSystem.Item>
{
    [SerializeField]
    private ItemSlot icon;

    [SerializeField]
    private Text nameText;

    [SerializeField]
    private GameObject selected;

    public override void Refresh()
    {
        icon.SetItem(Data);
        nameText.text = Data.Name;
    }

    protected override void RefreshSelected()
    {
        ZetanUtility.SetActive(selected, isSelected);
    }

    public override void Clear()
    {
        base.Clear();
        nameText.text = string.Empty;
        Data = null;
        icon.Vacate();
    }
}
