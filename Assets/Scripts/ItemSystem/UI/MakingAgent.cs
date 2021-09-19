using UnityEngine;
using UnityEngine.UI;

public class MakingAgent : MonoBehaviour
{
    [SerializeField]
    private Text nameText;

    public ItemBase MItem { get; private set; }

    public void OnClick()
    {
        if (MItem)
        {
            MakingManager.Instance.ShowDescription(MItem);
        }
    }

    public void Init(ItemBase item)
    {
        if (!item) return;
        this.MItem = item;
        nameText.text = item.name;
    }

    public void Clear(bool recycle = false)
    {
        nameText.text = string.Empty;
        MItem = null;
        if (recycle) ObjectPool.Put(gameObject);
    }
}
