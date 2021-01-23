using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class UIGridMaker : MonoBehaviour
{
    public UIGrid gridPrefab;
    public List<UIGrid> grids;

    private ScrollRect scrollRect;
    private RectTransform rectTransform;
    private RectTransform content;
    private RectTransform viewport;
    private GridLayoutGroup grid;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        rectTransform = GetComponent<RectTransform>();
        if (!scrollRect.viewport)
        {
            GameObject vp = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            scrollRect.viewport = vp.GetComponent<RectTransform>();
            scrollRect.viewport.SetParent(rectTransform, false);
            scrollRect.viewport.anchorMin = Vector2.zero;
            scrollRect.viewport.anchorMax = Vector2.one;
            scrollRect.viewport.sizeDelta = Vector2.zero;
        }
        viewport = scrollRect.viewport;
        if (!scrollRect.content)
        {
            GameObject ct = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            scrollRect.content = ct.GetComponent<RectTransform>();
            scrollRect.content.SetParent(scrollRect.viewport, false);
            scrollRect.content.anchorMin = Vector2.up;
            scrollRect.content.anchorMax = Vector2.one;
            scrollRect.content.sizeDelta = Vector2.zero;
            scrollRect.content.pivot = new Vector2(0.5f, 1f);
            ContentSizeFitter csf = scrollRect.content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            grid = scrollRect.content.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        }
        content = scrollRect.content;
    }
    public void Init(Vector2 size)
    {
        Rect realRect = ZetanUtility.GetScreenSpaceRect(scrollRect.viewport);
        grid.constraintCount = Mathf.CeilToInt((realRect.size.x + grid.spacing.x) / (size.x + grid.spacing.x));
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
