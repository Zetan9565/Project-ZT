using UnityEngine;

public class TestItem : MonoBehaviour
{
    public ItemBase item;

    public void OnClick()
    {
        BackpackManager.Instance.GetItem(item);
    }
}
