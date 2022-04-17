using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MaxAttribute))]
public class MaxAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MaxAttribute max = (MaxAttribute)attribute;
        if (fieldInfo.FieldType == typeof(int))
        {
            if (property.intValue> max.intValue)
                property.intValue = max.intValue;
        }
        else if (fieldInfo.FieldType == typeof(long))
        {
            switch (max.valueType)
            {
                case MaxAttribute.ValueType.Int:
                    if (property.longValue > max.intValue)
                        property.longValue = max.intValue;
                    break;
                case MaxAttribute.ValueType.Long:
                    if (property.longValue > max.longValue)
                        property.longValue = max.longValue;
                    break;
            }
        }
        else if (fieldInfo.FieldType == typeof(float))
        {
            switch (max.valueType)
            {
                case MaxAttribute.ValueType.Int:
                    if (property.floatValue > max.intValue)
                        property.floatValue = max.intValue;
                    break;
                case MaxAttribute.ValueType.Long:
                    if (property.floatValue > max.longValue)
                        property.floatValue = max.longValue;
                    break;
                case MaxAttribute.ValueType.Float:
                    if (property.floatValue > max.floatValue)
                        property.floatValue = max.floatValue;
                    break;
            }
        }
        else if(fieldInfo.FieldType == typeof(double))
        {
            switch (max.valueType)
            {
                case MaxAttribute.ValueType.Int:
                    if (property.doubleValue > max.intValue)
                        property.doubleValue = max.intValue;
                    break;
                case MaxAttribute.ValueType.Long:
                    if (property.doubleValue > max.longValue)
                        property.doubleValue = max.longValue;
                    break;
                case MaxAttribute.ValueType.Float:
                    if (property.doubleValue > max.floatValue)
                        property.doubleValue = max.floatValue;
                    break;
                case MaxAttribute.ValueType.Double:
                    if (property.doubleValue > max.doubleValue)
                        property.doubleValue = max.doubleValue;
                    break;
                default:
                    break;
            }
        }
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }
}