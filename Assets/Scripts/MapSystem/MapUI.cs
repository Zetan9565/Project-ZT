using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    public CanvasGroup mapWindow;

    public RectTransform mapWindowRect;

    public MapIcon iconPrefb;
    public RectTransform iconsParent;

    public RectTransform mapRect;
    public RawImage mapImage;

    public Button @switch;
    public Button locate;

    private void Awake()
    {
        @switch.onClick.AddListener(MapManager.Instance.SwitchMapMode);
        locate.onClick.AddListener(MapManager.Instance.LocatePlayer);
    }
}