using UnityEngine;
using ZetanStudio.InventorySystem;
using ZetanStudio.ItemSystem;

public class TestItem : MonoBehaviour
{
    public Item item;

    public void OnClick()
    {
        BackpackManager.Instance.Get(item, 1);
    }
}
