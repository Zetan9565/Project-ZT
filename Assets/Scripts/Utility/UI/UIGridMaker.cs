//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UI;

//[RequireComponent(typeof(ScrollRect))]
//public class UIGridMaker : MonoBehaviour
//{
//    public UIGrid gridPrefab;
//    public List<UIGrid> grids = new List<UIGrid>();
//    public List<object> data = new List<object>();

//    private ScrollRect scrollRect;
//    private RectTransform rectTransform;
//    private int maxGridCount;

//    public RectOffset padding;
//    public Vector2 spacing;
//    public Vector2 cellSize;

//    [SerializeField, ReadOnly]
//    private int constraintCount;
//    [SerializeField, ReadOnly]
//    private int rowCount;

//    private void Awake()
//    {
//        scrollRect = GetComponent<ScrollRect>();
//        rectTransform = GetComponent<RectTransform>();
//        if (!scrollRect.viewport)
//        {
//            GameObject vp = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
//            scrollRect.viewport = vp.GetComponent<RectTransform>();
//            scrollRect.viewport.SetParent(rectTransform, false);
//            scrollRect.viewport.anchorMin = Vector2.zero;
//            scrollRect.viewport.anchorMax = Vector2.one;
//            scrollRect.viewport.sizeDelta = Vector2.zero;
//        }
//        if (!scrollRect.content)
//        {
//            GameObject ct = new GameObject("Grid", typeof(RectTransform));
//            scrollRect.content = ct.GetComponent<RectTransform>();
//            scrollRect.content.SetParent(scrollRect.viewport, false);
//            scrollRect.content.anchorMin = Vector2.up;
//            scrollRect.content.anchorMax = Vector2.one;
//            scrollRect.content.sizeDelta = Vector2.zero;
//            scrollRect.content.pivot = new Vector2(0.5f, 1f);
//        }
//        scrollRect.onValueChanged.AddListener(CheckGridPosition2);
//        Init(new Vector2(80, 80));
//        ItemInfo[] test = new ItemInfo[80];
//        for (int i = 0; i < test.Length; i++)
//        {
//            test[i] = new ItemInfo
//            {
//                Amount = i
//            };
//        }
//        SetData(test);
//    }
//    public void Init(Vector2 size)
//    {
//        constraintCount = Mathf.FloorToInt((scrollRect.viewport.rect.width - padding.left - padding.right + spacing.x) / (size.x + spacing.x));
//        cellSize = size;
//        rowCount = Mathf.CeilToInt((scrollRect.viewport.rect.height - padding.top - padding.bottom + spacing.y) / (size.y + spacing.y));
//        maxGridCount = constraintCount * (rowCount + 1);
//        while (grids.Count < maxGridCount)
//        {
//            UIGrid grid = ObjectPool.Get(gridPrefab, scrollRect.content);
//            grid.rectTransform.anchorMin = Vector2.up;
//            grid.rectTransform.anchorMax = Vector2.up;
//            grid.rectTransform.pivot = Vector2.up;
//            grids.Add(grid);
//        }
//        for (int i = 0; i < grids.Count; i++)
//        {
//            grids[i].rectTransform.sizeDelta = size;
//            grids[i].rectTransform.SetSiblingIndex(i);
//        }
//    }

//    public void SetData(IEnumerable<object> data)
//    {
//        if (data == null)
//            data = new object[0];
//        this.data = data.ToList();
//        dataStartIndex = 0;
//        dataStartIndexBef = -1;
//        int mod = this.data.Count % constraintCount;
//        dataLastIndex = this.data.Count + constraintCount - (mod == 0 ? constraintCount : mod) - 1;
//        maxStartIndex = dataLastIndex - grids.Count + 1;
//        scrollRect.StopMovement();
//        scrollRect.content.anchoredPosition = Vector2.zero;
//        int dataLength = data.Count();
//        int row = Mathf.CeilToInt(dataLength * 1.0f / constraintCount);
//        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x,
//            padding.top + padding.bottom + (cellSize.y + spacing.y) * row - spacing.y);
//        RefreshData();
//    }

//    public void RefreshData(IEnumerable<object> data = null)
//    {
//        if (data != null)
//            this.data = data.ToList();
//        for (int i = 0; i < grids.Count; i++)
//        {
//            grids[i].rectTransform.anchoredPosition = CalGridPos(dataStartIndex + i);
//            grids[i].dataIndex = dataStartIndex + i;
//            grids[i].RefreshData(this.data[dataStartIndex + i]);
//        }
//    }

//    private int dataStartIndex = 0;
//    private int dataStartIndexBef = 0;
//    private int dataLastIndex = 0;
//    private int maxStartIndex = 0;
//    public void CheckGridPosition(Vector2 move)
//    {
//        //Debug.Log($"滚动：{scrollRect.velocity.y}");
//        Vector3[] vCorners = new Vector3[4];
//        rectTransform.GetWorldCorners(vCorners);
//        if (grids[constraintCount])//第二行第一个
//        {
//            //  1 ┏━┓ 2
//            //  0 ┗━┛ 3
//            Vector3[] gCorners = new Vector3[4];
//            grids[constraintCount].rectTransform.GetWorldCorners(gCorners);
//            if (gCorners[0].y > vCorners[1].y)
//            {
//                float scrollOffset = scrollRect.content.anchoredPosition.y - padding.top;
//                int row = Mathf.CeilToInt(scrollOffset / (cellSize.y + spacing.y)) - 2;
//                row = row < 0 ? 0 : row;
//                dataStartIndex = row * constraintCount;
//                dataStartIndex = dataStartIndex > maxStartIndex ? maxStartIndex : dataStartIndex;
//                if (grids[0].dataIndex < dataStartIndex && dataStartIndex <= maxStartIndex && move.y > 0 && move.y < 1)
//                {
//                    Debug.Log($"顶部越界了: {dataStartIndex}");
//                    int swapTimes = Mathf.CeilToInt((gCorners[0].y - vCorners[1].y) / (cellSize.y + spacing.y));
//                    int offset = constraintCount * (swapTimes - 1);
//                    if (swapTimes > 1)
//                        Debug.Log($"顶部超过1次：{swapTimes}");
//                    Reverse(0, constraintCount * swapTimes - 1);
//                    Reverse(constraintCount * swapTimes, grids.Count - 1);
//                    Reverse(0, grids.Count - 1);
//                    for (int i = grids.Count - constraintCount * swapTimes; i < grids.Count; i++)
//                    {
//                        grids[i].rectTransform.SetSiblingIndex(i);
//                        grids[i].rectTransform.anchoredPosition = CalGridPos(dataStartIndex + i - offset);
//                        grids[i].dataIndex = dataStartIndex + i - offset;
//                        grids[i].RefreshData(data[dataStartIndex + i - offset]);
//                    }
//                }
//                return;
//            }
//        }
//        if (grids[grids.Count - constraintCount * 2])//倒数第二行第一个
//        {
//            Vector3[] gCorners = new Vector3[4];
//            grids[grids.Count - constraintCount * 2].rectTransform.GetWorldCorners(gCorners);
//            if (gCorners[1].y < vCorners[0].y)
//            {
//                float scrollOffset = scrollRect.content.anchoredPosition.y - padding.top;
//                int row = Mathf.CeilToInt(scrollOffset / (cellSize.y + spacing.y)) - 2;
//                row = row < -1 ? -1 : row;
//                dataStartIndex = (row + 1) * constraintCount;
//                dataStartIndex = dataStartIndex > maxStartIndex ? maxStartIndex : dataStartIndex;
//                if (grids[0].dataIndex > dataStartIndex && scrollRect.content.anchoredPosition.y > 0)
//                {
//                    Debug.Log($"底部越界了: {dataStartIndex}");
//                    int swapTimes = Mathf.CeilToInt((vCorners[0].y - gCorners[1].y) / (cellSize.y + spacing.y));
//                    int offset = constraintCount * (swapTimes - 1);
//                    if (swapTimes > 1)
//                        Debug.Log($"底部超过1次：{swapTimes}");
//                    Reverse(0, grids.Count - 1);
//                    Reverse(0, constraintCount * swapTimes - 1);
//                    Reverse(constraintCount * swapTimes, grids.Count - 1);
//                    for (int i = 0; i < constraintCount * swapTimes; i++)
//                    {
//                        grids[i].rectTransform.SetSiblingIndex(i);
//                        grids[i].rectTransform.anchoredPosition = CalGridPos(dataStartIndex + i - offset);
//                        grids[i].dataIndex = dataStartIndex + i - offset;
//                        grids[i].RefreshData(data[dataStartIndex + i - offset]);
//                    }
//                }
//            }
//        }

//        void Reverse(int start, int end)
//        {
//            while (start < end)
//            {
//                var temp = grids[end];
//                grids[end] = grids[start];
//                grids[start] = temp;
//                start++;
//                end--;
//            }
//        }
//    }

//    public void CheckGridPosition2(Vector2 move)
//    {
//        //Debug.Log($"滚动：{scrollRect.velocity.y}");
//        Vector3[] vCorners = new Vector3[4];
//        rectTransform.GetWorldCorners(vCorners);
//        CalStartIndex();
//        if (grids[0])//第二行第一个
//        {
//            //  1 ┏━┓ 2
//            //  0 ┗━┛ 3
//            Vector3[] gCorners = new Vector3[4];
//            grids[0].rectTransform.GetWorldCorners(gCorners);
//            if (gCorners[0].y > vCorners[1].y && scrollRect.velocity.y > 0)
//            {
//                if (grids[0].dataIndex < dataStartIndex && dataStartIndex <= maxStartIndex && move.y > 0 && move.y < 1)
//                {
//                    Debug.Log($"顶部越界了: {dataStartIndex}");
//                    int swapTimes = Mathf.CeilToInt((gCorners[0].y - vCorners[1].y) / (cellSize.y + spacing.y));
//                    int offset = constraintCount * (swapTimes - 1);
//                    if (swapTimes > 1)
//                        Debug.Log($"顶部超过1次：{swapTimes}");
//                    Reverse(0, constraintCount * swapTimes - 1);
//                    Reverse(constraintCount * swapTimes, grids.Count - 1);
//                    Reverse(0, grids.Count - 1);
//                    for (int i = grids.Count - constraintCount * swapTimes; i < grids.Count; i++)
//                    {
//                        grids[i].rectTransform.SetSiblingIndex(i);
//                        grids[i].rectTransform.anchoredPosition = CalGridPos(dataStartIndex + i - offset);
//                        grids[i].dataIndex = dataStartIndex + i - offset;
//                        grids[i].RefreshData(data[dataStartIndex + i - offset]);
//                    }
//                }
//            }
//            else if (gCorners[1].y < vCorners[1].y && scrollRect.velocity.y < 0)
//            {
//                if (grids[0].dataIndex > dataStartIndex && scrollRect.content.anchoredPosition.y > 0)
//                {
//                    Debug.Log($"底部越界了: {dataStartIndex}");
//                    int swapTimes = Mathf.CeilToInt((vCorners[1].y - gCorners[1].y) / (cellSize.y + spacing.y));
//                    int offset = constraintCount * (swapTimes - 1);
//                    if (swapTimes > 1)
//                        Debug.Log($"底部超过1次：{swapTimes}");
//                    Reverse(0, grids.Count - 1);
//                    Reverse(0, constraintCount * swapTimes - 1);
//                    Reverse(constraintCount * swapTimes, grids.Count - 1);
//                    for (int i = 0; i < constraintCount * swapTimes; i++)
//                    {
//                        grids[i].rectTransform.SetSiblingIndex(i);
//                        grids[i].rectTransform.anchoredPosition = CalGridPos(dataStartIndex + i - offset);
//                        grids[i].dataIndex = dataStartIndex + i - offset;
//                        grids[i].RefreshData(data[dataStartIndex + i - offset]);
//                    }
//                }
//            }
//        }

//        void Reverse(int start, int end)
//        {
//            while (start < end)
//            {
//                var temp = grids[end];
//                grids[end] = grids[start];
//                grids[start] = temp;
//                start++;
//                end--;
//            }
//        }
//    }

//    private Vector2 CalGridPos(int index)
//    {
//        index++;
//        int row = Mathf.CeilToInt(index * 1.0f / constraintCount);
//        int col = index % constraintCount;
//        col = col == 0 ? constraintCount : col;
//        return new Vector2(padding.left + (cellSize.x + spacing.x) * (col - 1), -(padding.top + (cellSize.y + spacing.y) * (row - 1)));
//    }

//    private void CalStartIndex()
//    {
//        float scrollOffset = scrollRect.content.anchoredPosition.y - padding.top;
//        int row = Mathf.CeilToInt(scrollOffset / (cellSize.y + spacing.y)) - 1;
//        row = row < 0 ? 0 : row;
//        dataStartIndex = row * constraintCount;
//        dataStartIndex = dataStartIndex > maxStartIndex ? maxStartIndex : dataStartIndex;
//    }

//    private void CalLastIndex()
//    {

//    }

//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
