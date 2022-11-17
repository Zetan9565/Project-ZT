using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;
using ZetanStudio.PlayerSystem;

public class MapUI : MonoBehaviour
{
    public CanvasGroup mapWindow;

    public RectTransform mapWindowRect;
    public RectTransform mapMaskRect;

    public MapIcon iconPrefab;
    public RectTransform iconsParent;
    public RectTransform mainParent;

    public RectTransform marksParent;
    public RectTransform objectivesParent;
    public RectTransform questsParent;

    public RectTransform mapRect;
    public RawImage mapImage;

    //仅用于调试
    public Button cancelPath;
    //仅用于调试
    public Button followPath;
    public Button @switch;
    public Button locate;

    private void Awake()
    {
        cancelPath.onClick.AddListener(PlayerManager.Instance.ResetPath);
        followPath.onClick.AddListener(PlayerManager.Instance.Trace);
        @switch.onClick.AddListener(MapManager.Instance.SwitchMapMode);
        locate.onClick.AddListener(MapManager.Instance.LocatePlayer);
    }
}