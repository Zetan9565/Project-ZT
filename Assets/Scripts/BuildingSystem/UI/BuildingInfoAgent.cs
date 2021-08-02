using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class BuildingInfoAgent : MonoBehaviour,
    IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Text nameText;

    private ScrollRect parentRect;

    public BuildingInformation Info { get; private set; }

    public void Init(BuildingInformation buildingInfo, ScrollRect parentRect = null)
    {
        Info = buildingInfo;
        nameText.text = buildingInfo.name;
        this.parentRect = parentRect;
    }

    public void Clear(bool recycle = false)
    {
        nameText.text = string.Empty;
        Info = null;
        if (recycle) ObjectPool.Put(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        if(eventData.button == PointerEventData.InputButton.Right)
            TryBuild();
        else if(eventData.button == PointerEventData.InputButton.Left)
            BuildingManager.Instance.ShowBuiltList(MBuildingInfo);
#elif UNITY_ANDROID
        if (touchTime < 0.5f)
        {
            BuildingManager.Instance.ShowDescription(Info);
            BuildingManager.Instance.ShowBuiltList(Info);
        }
#endif
    }

    private float touchTime;
    private bool isPress;

    private void Update()
    {
#if UNITY_ANDROID
        if (isPress)
        {
            touchTime += Time.deltaTime;
            if (touchTime >= 0.5f)
            {
                isPress = false;
                OnLongPress();
            }
        }
#endif
    }

    void TryBuild()
    {
        if (!BackpackManager.Instance.IsMaterialsEnough(Info.Materials))
        {
            MessageManager.Instance.New("耗材不足");
            return;
        }
        else BuildingManager.Instance.CreatPreview(Info);
    }

    public void OnLongPress()
    {
#if UNITY_ANDROID
        TryBuild();
#endif
    }

    public void OnPointerDown(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            touchTime = 0;
            isPress = true;
        }
#endif
    }

    public void OnPointerUp(PointerEventData eventData)
    {
#if UNITY_ANDROID
        isPress = false;
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        BuildingManager.Instance.ShowDescription(MBuildingInfo);
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        BuildingManager.Instance.HideDescription();
#endif
#if UNITY_ANDROID
        isPress = false;
#endif
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (parentRect) parentRect.OnBeginDrag(eventData);
#endif
    }

    public void OnDrag(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (BuildingManager.Instance.IsPreviewing && eventData.button == PointerEventData.InputButton.Left)
            BuildingManager.Instance.ShowAndMovePreview();
        else if (parentRect) parentRect.OnDrag(eventData);
#endif
    }

    public void OnEndDrag(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (parentRect) parentRect.OnEndDrag(eventData);
        if (BuildingManager.Instance.IsPreviewing && eventData.button == PointerEventData.InputButton.Left)
            BuildingManager.Instance.DoPlace();
#endif
    }
}
