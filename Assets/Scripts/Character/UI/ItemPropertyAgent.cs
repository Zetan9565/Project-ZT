using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.ItemSystem;
using ZetanStudio;
using ZetanStudio.CharacterSystem;

public class ItemPropertyAgent : MonoBehaviour
{
    [SerializeField]
    private Text leftName;
    [SerializeField]
    private Text leftValue;

    //[SerializeField]
    //private Text rightValue;

    //[SerializeField]
    //private GameObject arrow;

    public void Init(ItemProperty left)
    {
        Clear();
        if (left)
        {
            if (!left.HasBoolValue)
            {
                leftName.text = left.Name;
                leftValue.text = left.ValueString;
            }
            else
            {
                leftName.text = (!left.BoolValue ? L.Tr(GetType().Name, "消除") : string.Empty) + left.Name;
            }
        }
    }

    //public void Init(ItemAttribute left, ItemAttribute right)
    //{
    //    Clear();
    //    if (left)
    //    {
    //        leftName.text = left.Name;
    //        if (!left.HasBoolValue)
    //        {
    //        ZetanUtility.SetActive(arrow, true);
    //            leftValue.text = $"{left.Value}";
    //            if (right)
    //            {
    //                rightValue.text = $"{right.Value}";
    //                float diff = (float)(dynamic)left.Value - (float)(dynamic)right.Value;
    //                if (diff < 0) rightValue.text += $"<color=#111111>({ZetanUtility.ColorText($"{diff}", Color.red)})</color>";
    //                else if (diff > 0) rightValue.text += $"<color=#111111>({ZetanUtility.ColorText($"+{diff}", Color.green)})</color>";
    //            }
    //        }
    //        else if (!right) rightValue.text = ZetanUtility.ColorText("-", Color.red);
    //    }
    //}

    public void Clear(bool recycle = false)
    {
        leftName.text = string.Empty;
        leftValue.text = string.Empty;
        //rightValue.text = string.Empty;
        //ZetanUtility.SetActive(arrow, false);
        if (recycle) ObjectPool.Put(gameObject);
    }
}