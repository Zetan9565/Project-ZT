using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DragableManager : MonoBehaviour
{
    private static DragableManager instance;
    public static DragableManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<DragableManager>();
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

    public UnityEvent onCancelDrag;

    [SerializeField]
    private Image icon;

    // Start is called before the first frame update
    /*void Start()
    {
        icon = GetComponent<Image>();
    }*/

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

    public void GetDragable(IDragable dragable, UnityAction cancelDragAction = null)
    {
        if (!dragable.DragableIcon) return;
        Current = dragable;
        icon.overrideSprite = dragable.DragableIcon;
        icon.color = Color.white;
        MyTools.SetActive(icon.gameObject, true);
        onCancelDrag.RemoveAllListeners();
        if (cancelDragAction != null) onCancelDrag.AddListener(cancelDragAction);
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
