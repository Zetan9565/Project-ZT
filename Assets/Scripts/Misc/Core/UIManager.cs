using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;

public class UIManager : SingletonMonoBehaviour<UIManager>
{
    [SerializeField]
    private Transform questFlagParent;
    [SerializeField]
    private Transform structureFlagParent;

    public Transform QuestFlagParent
    {
        get
        {
            return questFlagParent ? questFlagParent : transform;
        }
    }

    public Transform StructureFlagParent
    {
        get
        {
            return structureFlagParent ? structureFlagParent : transform;
        }
    }

    private static bool dontDestroyOnLoadOnce;
    private void Awake()
    {
        if (!dontDestroyOnLoadOnce)
        {
            DontDestroyOnLoad(this);
            dontDestroyOnLoadOnce = true;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void Init()
    {

        DragableManager.Instance.ResetIcon();
        ProgressBar.Instance.Cancel();
    }
}
