using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIGrid : MonoBehaviour
{
    private RectTransform rectTransform;

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
        rectTransform = GetComponent<RectTransform>();
    }

    public void RefreshData()
    {

    }
}
