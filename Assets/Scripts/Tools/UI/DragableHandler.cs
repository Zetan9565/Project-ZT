using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class DragableHandler : MonoBehaviour
{
    private static DragableHandler instance;
    public static DragableHandler Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<DragableHandler>();
            return instance;
        }
    }

    public IDragable Current { get; private set; }

    public bool IsDraging
    {
        get
        {
            return Current != null;
        }
    }

    [HideInInspector]
    public UnityEvent onCancelDrag;

    private Image icon;

    private Canvas iconSortCanvas;

    private void Awake()
    {
        icon = GetComponent<Image>();
        if (!icon.GetComponent<GraphicRaycaster>()) icon.gameObject.AddComponent<GraphicRaycaster>();
        iconSortCanvas = icon.GetComponent<Canvas>();
        iconSortCanvas.overrideSorting = true;
    }

    public void Update()
    {
#if UNITY_STANDALONE
        MoveIcon();
#endif
    }

    // Update is called once per frame
    public void MoveIcon()
    {
        if (Current != null)
        {
            if (Input.GetMouseButtonDown(1))
            {
                CancelDrag();
            }
            icon.transform.position = Input.mousePosition;
        }
    }

    public void GetDragable(IDragable dragable, UnityAction cancelDragAction = null, float width = 100, float height = 100)
    {
        if (!dragable.DragableIcon) return;
        iconSortCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        Current = dragable;
        icon.overrideSprite = dragable.DragableIcon;
        icon.color = Color.white;
        MyTools.SetActive(icon.gameObject, true);
        onCancelDrag.RemoveAllListeners();
        if (cancelDragAction != null) onCancelDrag.AddListener(cancelDragAction);
        icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        MoveIcon();
    }

    public void ResetIcon()
    {
        Current = null;
        MyTools.SetActive(icon.gameObject, false);
    }

    void CancelDrag()
    {
        ResetIcon();
        onCancelDrag?.Invoke();
    }
}