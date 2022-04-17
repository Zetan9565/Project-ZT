using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
public class EmptyGraphic : Graphic
{
    public override void SetMaterialDirty()
    {
        return;
    }
    public override void SetVerticesDirty()
    {
        return;
    }
}
