using UnityEngine;
using UnityEngine.UI;

public class MakingAgent : MonoBehaviour
{
    [SerializeField]
    private Text nameText;

    private ItemBase item;

    public void OnClick()
    {
        if (item)
        {
            MakingManager.Instance.ShowDescription(item);
        }
    }

    public void Init(ItemBase item)
    {
        if (!item) return;
        this.item = item;
        nameText.text = item.name;
    }

    public void Clear(bool recycle = false)
    {
        nameText.text = string.Empty;
        item = null;
        if (recycle) ObjectPool.Instance.Put(gameObject);
    }
}
