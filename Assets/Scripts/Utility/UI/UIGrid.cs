using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIGrid : MonoBehaviour
{
    public RectTransform rectTransform;

    public UnityEngine.UI.Text text;

    public int dataIndex = -1;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
    }

    public void Init(RectTransform parent, float size)
    {
    }

    public void RefreshData(object data)
    {
        if (data is ItemInfo info)
        {
            text.text = dataIndex.ToString();
        }
    }
}
