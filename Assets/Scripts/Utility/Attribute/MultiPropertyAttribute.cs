using UnityEngine;

public class MultiPropertyAttribute : PropertyAttribute
{
    public readonly string[] labels;

    public MultiPropertyAttribute(string[] labels)
    {
        this.labels = labels;
    }
}