using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class QuestAgent : MonoBehaviour
{
    [HideInInspector]
    public Quest MQuest;
#if UNITY_EDITOR
    [SerializeField, DisplayName("标题文字")]
#endif
    private Text TitleText;
#if UNITY_EDITOR
    [SerializeField, DisplayName("隶属于完成列表", true)]
#endif
    private bool belongToCmplt;
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("选中特效")]
#endif
    private Outline selectedOutline;
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("选中颜色")]
#endif
    private Color selectedColor = Color.yellow;
    /// <summary>
    /// 使用前的初始化
    /// </summary>
    /// <param name="quest">对应任务</param>
    /// <param name="isFinished">任务完成情况</param>
    public void Init(Quest quest, bool isFinished = false)
    {
        if (!selectedOutline.effectColor.Equals(selectedColor)) selectedOutline.effectColor = selectedColor;
        MQuest = quest;
        belongToCmplt = isFinished;
        Deselect();
        UpdateQuestStatus();
    }
    /// <summary>
    /// 回收
    /// </summary>
    public void Recycle()
    {
        MQuest = null;
        TitleText.text = string.Empty;
        Deselect();
        belongToCmplt = false;
        ObjectPool.Instance.Put(gameObject);
    }

    public void UpdateQuestStatus()
    {
        if (MQuest)
        {
            if (!belongToCmplt) TitleText.text = MQuest.Title + (MQuest.IsComplete ? "(完成)" : string.Empty);
            else TitleText.text = MQuest.Title;
        }
    }

    public void OnClick()
    {
        if (!MQuest) return;
        QuestManager.Instance.OpenDescriptionWindow(this);
    }

    public void Select()
    {
        if (!selectedOutline.enabled) selectedOutline.enabled = true;
    }

    public void Deselect()
    {
        if (selectedOutline.enabled) selectedOutline.enabled = false;
    }
}
