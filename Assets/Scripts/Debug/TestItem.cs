using UnityEngine;

public class TestItem : MonoBehaviour
{
    [ZetanStudio.Item.ItemSelector]
    public ItemBase item;

    public void OnClick()
    {
        BackpackManager.Instance.GetItem(item, 1);
    }
}
