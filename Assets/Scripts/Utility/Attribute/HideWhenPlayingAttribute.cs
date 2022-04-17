using System.Collections;
using UnityEngine;

public class HideWhenPlayingAttribute : PropertyAttribute
{
    public readonly bool readOnly;
    public readonly bool reverse;

    public HideWhenPlayingAttribute(bool readOnly = false, bool reverse=false)
    {
        this.readOnly = readOnly;
        this.reverse = reverse;
    }
}