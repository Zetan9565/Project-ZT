using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute
{
    //The name of the field that will be in control
    public string conditionalSourceField = string.Empty;
    //TRUE = Negate the bool field that will be in control
    public bool Negate;
    //Record the value if the field is enum type, the value of enum element must be power of 2
    public int enumCondition;
    //TRUE = Hide in inspector / FALSE = Disable in inspector 
    public bool hideInInspector = false;

    public ConditionalHideAttribute(string conditionalSourceField, bool negate = false, bool hideInInspector = true)
    {
        this.conditionalSourceField = conditionalSourceField;
        this.hideInInspector = hideInInspector;
        Negate = negate;
    }

    public ConditionalHideAttribute(string conditionalSourceField, int enumCondition, bool hideInInspector = false)
    {
        this.conditionalSourceField = conditionalSourceField;
        this.hideInInspector = hideInInspector;
        this.enumCondition = enumCondition;
    }
}