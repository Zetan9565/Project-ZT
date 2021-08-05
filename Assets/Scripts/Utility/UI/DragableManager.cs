using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class DragableManager : SingletonMonoBehaviour<DragableManager>
{
    public IDragAble Current { get; private set; }

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
#if UNITY_STANDALONE

    private void Update()
    {
        MoveIcon();
    }
#endif

    // Update is called once per frame
    public void MoveIcon()
    {
        if (Current != null)
        {
            if (Input.GetMouseButtonDown(1) || Input.touches.Length > 1)
            {
                CancelDrag();
            }
            icon.transform.position = Input.mousePosition;
        }
    }

    public void StartDrag(IDragAble dragable, UnityAction cancelDragAction = null, float width = 100, float height = 100)
    {
        if (!dragable.DragAbleIcon) return;
        iconSortCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        Current = dragable;
        icon.overrideSprite = dragable.DragAbleIcon;
        icon.color = Color.white;
        ZetanUtility.SetActive(icon.gameObject, true);
        onCancelDrag.RemoveAllListeners();
        if (cancelDragAction != null) onCancelDrag.AddListener(cancelDragAction);
        icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        MoveIcon();
    }

    public void ResetIcon()
    {
        Current = null;
        ZetanUtility.SetActive(icon.gameObject, false);
    }

    public void CancelDrag()
    {
        ResetIcon();
        onCancelDrag?.Invoke();
    }
}