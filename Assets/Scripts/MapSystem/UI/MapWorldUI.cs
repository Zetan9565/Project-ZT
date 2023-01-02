using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;

public class MapWorldUI : Window, IMapUI
{
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

    public Button locate;

    protected override void OnAwake()
    {
        locate.onClick.AddListener(MapManager.Instance.LocatePlayer);
        
    }

    public override void OnCloseComplete()
    {
        base.OnCloseComplete();
    }
}