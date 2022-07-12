using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ZetanStudio.Extension.Editor;

public abstract class EnhancedAttributeDrawer : PropertyDrawer
{
    private PropertyDrawer custom;
    private bool shouldCheckCustom = true;
    private List<EnhancedPropertyAttribute> attrs;

    private bool IsLast()
    {
        return Attrs() is List<EnhancedPropertyAttribute> attrs && attrs.IndexOf(attribute as EnhancedPropertyAttribute) == attrs.Count - 1;

        List<EnhancedPropertyAttribute> Attrs()
        {
            return this.attrs ??= getFieldAttributes(fieldInfo);

            static List<EnhancedPropertyAttribute> getFieldAttributes(FieldInfo field)
            {
                if (field == null) return null;
                if (field.GetCustomAttributes<EnhancedPropertyAttribute>(true) is IEnumerable<EnhancedPropertyAttribute> customAttributes && customAttributes.Count() != 0)
                    return new List<EnhancedPropertyAttribute>(customAttributes.OrderBy(x => x.order));
                else return null;
            }
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        PropertyField(position, property, label);
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return PropertyHeight(property, label);
    }

    protected void PropertyField(Rect position, SerializedProperty property, GUIContent label, bool includeChildren = false)
    {
        if (IsLast() && TryGetCustomDrawer() is PropertyDrawer custom) custom.OnGUI(position, property, label);
        else EditorGUI.PropertyField(position, property, label, includeChildren);
    }
    protected float PropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (IsLast() && TryGetCustomDrawer() is PropertyDrawer custom) return custom.GetPropertyHeight(property, label);
        else return EditorGUI.GetPropertyHeight(property, label);
    }
    private PropertyDrawer TryGetCustomDrawer()
    {
        if (shouldCheckCustom)
        {
            custom = this.GetCustomDrawer();
            shouldCheckCustom = false;
        }
        return custom;
    }
}