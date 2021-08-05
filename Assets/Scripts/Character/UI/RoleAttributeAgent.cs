using UnityEngine;
using UnityEngine.UI;

public class RoleAttributeAgent : MonoBehaviour
{
    [SerializeField]
    private Text leftName;
    [SerializeField]
    private Text leftValue;

    [SerializeField]
    private Text rightName;
    [SerializeField]
    private Text rightValue;

    [SerializeField]
    private GameObject arrow;

    public void Init(RoleAttribute left, RoleAttribute right = null)
    {
        Clear();
        if (left)
        {
            leftName.text = left.name;
            if (!RoleAttribute.IsUsingBoolValue(left))
                leftValue.text = left.Value.ToString();
        }
        if (right)
        {
            rightName.text = right.name;
            if (!RoleAttribute.IsUsingBoolValue(right))
                rightValue.text = right.Value.ToString();
        }
        ZetanUtility.SetActive(arrow, right != null);
    }

    public void Clear(bool recycle = false)
    {
        leftName.text = string.Empty;
        leftValue.text = string.Empty;
        rightName.text = string.Empty;
        rightValue.text = string.Empty;
        if (recycle) ObjectPool.Put(gameObject);
    }
}