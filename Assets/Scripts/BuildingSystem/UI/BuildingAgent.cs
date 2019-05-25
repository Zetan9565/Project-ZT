using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class BuildingAgent : MonoBehaviour, 
    IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, 
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Text nameText;

    private ScrollRect parentRect;

    public BuildingInfomation MBuildingInfo { get; private set; }

    public void Init(BuildingInfomation buildingInfo, ScrollRect parentRect = null)
    {
        MBuildingInfo = buildingInfo;
        nameText.text = buildingInfo.Name;
        this.parentRect = parentRect;
    }

    public void Clear(bool recycle = false)
    {
        nameText.text = string.Empty;
        MBuildingInfo = null;
        if (recycle) ObjectPool.Instance.Put(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_STANDALONE
        TryBuild();
#elif UNITY_ANDROID
        BuildingManager.Instance.ShowDescription(MBuildingInfo);
#endif
    }

    float touchTime;
    bool isPress;

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
        if (!MBuildingInfo.CheckMaterialsEnough(BackpackManager.Instance.MBackpack))
        {
            MessageManager.Instance.NewMessage("耗材不足");
            return;
        }
        else BuildingManager.Instance.CreatPreview(MBuildingInfo);
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
        BuildingManager.Instance.UnshowDescription();
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
        //if(eventData.pointerCurrentRaycast.gameObject.layer == LayerMask.NameToLayer("BuildAble"))
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (eventData.pointerCurrentRaycast.gameObject == BuildingManager.Instance.CancelArea && BuildingManager.Instance.IsPreviewing)
                BuildingManager.Instance.FinishPreview();
            else if (BuildingManager.Instance.IsPreviewing)
                BuildingManager.Instance.Build();
        }
#endif
    }
}
