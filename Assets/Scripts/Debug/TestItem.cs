using UnityEngine;
using ZetanStudio.Item;

public class TestItem : MonoBehaviour
{
    public Item item;

    public void OnClick()
    {
        BackpackManager.Instance.GetItem(item, 1);
    }
}
