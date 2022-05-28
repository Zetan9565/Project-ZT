using UnityEngine;

[CreateAssetMenu(fileName = "custom input", menuName = "Zetan Studio/其他/自定义按键方案")]
public class InputCustomInfo : ScriptableObject
{
    [SerializeField]
#if UNITY_EDITOR
    [Label("任务窗口按钮")]
#endif
    private KeyCode questWindowButton = KeyCode.O;

    [SerializeField]
#if UNITY_EDITOR
    [Label("交互按钮")]
#endif
    private KeyCode interactiveButton = KeyCode.R;

    [SerializeField]
#if UNITY_EDITOR
    [Label("背包按钮")]
#endif
    private KeyCode backpackButton = KeyCode.I;

    [SerializeField]
#if UNITY_EDITOR
    [Label("建筑按钮")]
#endif
    private KeyCode buildingButton = KeyCode.B;

    [SerializeField]
#if UNITY_EDITOR
    [Label("沿路行进按钮")]
#endif
    private KeyCode traceButton = KeyCode.T;

    public KeyCode QuestWindowButton
    {
        get
        {
            return questWindowButton;
        }
    }

    public KeyCode InteractiveButton
    {
        get
        {
            return interactiveButton;
        }
    }

    public KeyCode BackpackButton
    {
        get
        {
            return backpackButton;
        }
    }

    public KeyCode BuildingButton
    {
        get
        {
            return buildingButton;
        }
    }

    public KeyCode TraceButton
    {
        get
        {
            return traceButton;
        }
    }
}
