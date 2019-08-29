using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MapIcon : MonoBehaviour
{
    [HideInInspector]
    public Image iconImage;
    public Button iconButton;

    [HideInInspector]
    public MapIconType iconType;

    private void Awake()
    {
        iconImage = GetComponent<Image>();
        if (!iconButton) iconButton = GetComponent<Button>();
    }
}
public enum MapIconType
{
    Normal,
    Main,
    Mark,
    Quest,
}