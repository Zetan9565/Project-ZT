using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MessageAgent : MonoBehaviour
{
#if UNITY_EDITOR
    [Label("信息文字")]
#endif
    public Text messageText;
}