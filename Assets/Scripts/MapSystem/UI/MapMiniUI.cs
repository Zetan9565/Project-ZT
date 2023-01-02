using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;
using ZetanStudio.PlayerSystem;

public class MapMiniUI : MonoBehaviour, IMapUI
{
    public CanvasGroup mapWindow;

    [SerializeField] private RectTransform mapWindowRect;
    [SerializeField] private RectTransform mapMaskRect;

    [SerializeField] private RectTransform iconsParent;
    [SerializeField] private RectTransform mainParent;
    [SerializeField] private RectTransform rangeParent;
    [SerializeField] private RectTransform marksParent;
    [SerializeField] private RectTransform objectivesParent;
    [SerializeField] private RectTransform questsParent;

    [SerializeField] private RectTransform mapRect;
    [SerializeField] private RawImage mapImage;

    //仅用于调试
    public Button cancelPath;
    //仅用于调试
    public Button followPath;
    public Button @switch;
    public Button locate;

    public RectTransform MapWindowRect => mapWindowRect;
    public RectTransform MapMaskRect => mapMaskRect;
    public RectTransform IconsParent => iconsParent;
    public RectTransform MainParent => mainParent;
    public RectTransform RangeParent => rangeParent;
    public RectTransform MarksParent => marksParent;
    public RectTransform ObjectivesParent => objectivesParent;
    public RectTransform QuestsParent => questsParent;
    public RectTransform MapRect => mapRect;
    public RawImage MapImage => mapImage;

    private void Awake()
    {
        cancelPath.onClick.AddListener(PlayerManager.Instance.ResetPath);
        followPath.onClick.AddListener(PlayerManager.Instance.Trace);
        @switch.onClick.AddListener(MapManager.Instance.SwitchMapMode);
        locate.onClick.AddListener(MapManager.Instance.LocatePlayer);
    }
}

public interface IMapUI
{
    public RectTransform MapWindowRect { get; }
    public RectTransform MapMaskRect { get; }

    public RectTransform IconsParent { get; }
    public RectTransform MainParent { get; }
    public RectTransform RangeParent { get; }

    public RectTransform MarksParent { get; }
    public RectTransform ObjectivesParent { get; }
    public RectTransform QuestsParent { get; }

    public RectTransform MapRect { get; }
    public RawImage MapImage { get; }

    public GameObject gameObject { get; }
}