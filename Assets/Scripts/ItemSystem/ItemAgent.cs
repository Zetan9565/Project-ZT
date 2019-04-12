using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ItemAgent : MonoBehaviour {

    [SerializeField, DisplayName("图标")]
    private Image icon;
    public Image Icon
    {
        get
        {
            if (icon == null)
                icon = transform.Find("Icon").GetComponent<Image>();
            return icon;
        }
    }

    private ItemBase item;
    public ItemBase Item
    {
        get
        {
            return item;
        }

        set
        {
            item = value;
        }
    }

    public void OnClick()
    {
        //TODO 显示道具详情
    }
}
