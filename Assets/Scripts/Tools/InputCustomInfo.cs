using UnityEngine;

[CreateAssetMenu(fileName = "custom input", menuName = "ZetanStudio/其他/自定义按键方案")]
public class InputCustomInfo : ScriptableObject
{
    [SerializeField, DisplayName("任务窗口按钮")]
    private KeyCode questWindowButton = KeyCode.O;

    [SerializeField, DisplayName("交互按钮")]
    private KeyCode interactiveButton = KeyCode.R;

    public KeyCode QuestWindowButton
    {
        get
        {
            return questWindowButton;
        }
    }

    public KeyCode TalkButton
    {
        get
        {
            return interactiveButton;
        }
    }
}
