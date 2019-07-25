using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "story", menuName = "ZetanStudio/剧情/剧情")]
public class Story : ScriptableObject
{
    [SerializeField]
    private List<Plot> plots = new List<Plot>();
    public List<Plot> Plots
    {
        get
        {
            return plots;
        }
    }
}

[System.Serializable]
public class Plot
{
    [SerializeField]
    private string remark = string.Empty;
    public string Remark
    {
        get
        {
            return remark;
        }
    }

    [SerializeField]
    private List<PlotAction> actions = new List<PlotAction>();
    public List<PlotAction> Actions
    {
        get
        {
            return actions;
        }
    }
}

[System.Serializable]
public class PlotAction
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("空", "对话", "角色移动", "角色动画", "稍等", "相机移动", "相机抖动", "画面缩放", "画面闪烁")]
#endif
    private PlotActionType actionType;
    public PlotActionType ActionType
    {
        get
        {
            return actionType;
        }
    }

    [SerializeField]
    private Dialogue dialogue;
    public Dialogue Dialogue
    {
        get
        {
            return dialogue;
        }
    }

    [SerializeField]
    private bool forPlayer;
    public bool ForPlayer
    {
        get
        {
            return forPlayer;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("SetInt", "SetBool", "SetFloat", "SetTrigger", "播放动画片段")]
#endif
    private PlotAnimationType animaActionType;
    public PlotAnimationType AnimaActionType
    {
        get
        {
            return animaActionType;
        }
    }

    [SerializeField]
    private CharacterInformation character;
    public CharacterInformation Character
    {
        get
        {
            return character;
        }
    }

    [SerializeField]
    private string paramName = string.Empty;
    public string ParamName
    {
        get
        {
            return paramName;
        }
    }

    [SerializeField]
    private int intValue;
    public int IntValue
    {
        get
        {
            return intValue;
        }
    }

    [SerializeField]
    private bool boolValue;
    public bool BoolValue
    {
        get
        {
            return boolValue;
        }
    }

    [SerializeField]
    private float floatValue;
    public float FloatValue
    {
        get
        {
            return floatValue;
        }
    }

    [SerializeField]
    private AnimationClip animaClip;
    public AnimationClip AnimaClip
    {
        get
        {
            return animaClip;
        }
    }


    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("向上", "向下", "向左", "向右", "向左上", "向左下", "向右上", "向右下")]
#endif
    private PlotTransferDirection2D direction;
    public PlotTransferDirection2D Direction
    {
        get
        {
            return direction;
        }
    }

    [SerializeField]
    private float distance;
    public float Distance
    {
        get
        {
            return distance;
        }
    }

    [SerializeField]
    private float duration;
    public float Duration
    {
        get
        {
            return duration;
        }
    }

    [SerializeField]
    private float zoomMultiple;
    public float ZoomMultiple
    {
        get
        {
            return zoomMultiple;
        }
    }

    [SerializeField]
    private int extent = 1;
    public int Extent
    {
        get
        {
            return extent;
        }
    }

    [SerializeField]
    private int frequency = 1;
    public int Frequency
    {
        get
        {
            return frequency;
        }
    }
}

public enum PlotActionType
{
    None,
    Dialogue,
    TransferCharacter,
    //Rotate,
    Animation,
    WaitForSecond,
    TransferCamera,
    ShakeCamera,
    ZoomScreen,
    FlashScreen
}

//public enum PlotTransferDirection
//{
//    Forward,
//    Backward,
//    Left,
//    Right,
//    Up,
//    Down
//}
public enum PlotTransferDirection2D
{
    Up,//往上
    Down,//往下
    Left,//往左
    Right,//往右
    TopLeft,//往左上
    BottomLeft,//往左下
    TopRight,//往右上
    BottomRight//往右下
}

public enum PlotAnimationType
{
    SetInt,
    SetBool,
    SetFloat,
    SetTrigger,
    PlayClip
}