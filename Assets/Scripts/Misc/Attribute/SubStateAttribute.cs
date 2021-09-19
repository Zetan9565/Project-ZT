using UnityEngine;

public class SubStateAttribute : PropertyAttribute
{
    public string mainField;

    public SubStateAttribute(string mainField)
    {
        this.mainField = mainField;
    }
}