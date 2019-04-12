using UnityEngine;

public class CollectTestButton : MonoBehaviour
{

    public ItemBase item;
    public int amount = 1;
    [Tooltip("True = 获得，False = 丢弃")]
    public bool get = true;

    public void OnClick()
    {
        if (get)
            BagManager.Instance.GetItem(Instantiate(item), amount);
        else
        {
            BagManager.Instance.LoseItemByID(item.ID, amount);
        }
    }
}
