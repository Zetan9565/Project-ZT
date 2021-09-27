using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeedAgent : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Text nameText;

    public SeedItem Seed { get; private set; }

    private ScrollRect parentRect;

    public void Init(SeedItem seed, ScrollRect parentRect = null)
    {
        Seed = seed;
        nameText.text = Seed.Name;
    }

    public void Clear(bool recycle = false)
    {
        nameText.text = string.Empty;
        Seed = null;
        if (recycle) ObjectPool.Put(gameObject);
    }

    public void OnClick()
    {
        PlantManager.Instance.ShowDescription(Seed);
    }

    public void TryBuild()
    {
        PlantManager.Instance.CreatPreview(Seed.Crop);
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
        if (PlantManager.Instance.IsPreviewing && eventData.button == PointerEventData.InputButton.Left)
            PlantManager.Instance.ShowAndMovePreview();
        else if (parentRect) parentRect.OnDrag(eventData);
#endif
    }

    public void OnEndDrag(PointerEventData eventData)
    {
#if UNITY_ANDROID
        if (parentRect) parentRect.OnEndDrag(eventData);
        if (PlantManager.Instance.IsPreviewing && eventData.button == PointerEventData.InputButton.Left)
        {
            if (eventData.pointerCurrentRaycast.gameObject && eventData.pointerCurrentRaycast.gameObject == PlantManager.Instance.CancelArea)
                PlantManager.Instance.FinishPreview();
            else
                PlantManager.Instance.Plant();
        }
#endif
    }
}