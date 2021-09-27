using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool isVector2 = property.propertyType == SerializedPropertyType.Vector2;
        bool isVector2Int = property.propertyType == SerializedPropertyType.Vector2Int;
        if (isVector2 || isVector2Int)
        {
            MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
            float minLimit = 0;
            float maxLimit = 0;
            if (string.IsNullOrEmpty(attr.limitField) && (string.IsNullOrEmpty(attr.minLimitField) || string.IsNullOrEmpty(attr.maxLimitField)))
            {
                minLimit = attr.minLimit;
                maxLimit = attr.maxLimit;
            }
            else if (!string.IsNullOrEmpty(attr.limitField))
            {
                if (property.serializedObject == null || !property.serializedObject.targetObject)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    return;
                }
                SerializedProperty limitField = property.serializedObject.FindProperty(attr.limitField);
                if (limitField != null)
                {
                    if (limitField.propertyType == SerializedPropertyType.Vector2)
                    {
                        minLimit = limitField.vector2Value.x;
                        maxLimit = limitField.vector2Value.y;
                    }
                    else if (limitField.propertyType == SerializedPropertyType.Vector2Int)
                    {
                        minLimit = limitField.vector2IntValue.x;
                        maxLimit = limitField.vector2IntValue.y;
                    }
                    else
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                        return;
                    }
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(attr.minLimitField) && !string.IsNullOrEmpty(attr.maxLimitField))
            {
                if (property.serializedObject == null || !property.serializedObject.targetObject)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    return;
                }
                SerializedProperty minLimitField = property.serializedObject.FindProperty(attr.minLimitField);
                SerializedProperty maxLimitField = property.serializedObject.FindProperty(attr.maxLimitField);
                if (minLimitField != null && maxLimitField != null)
                {
                    if (minLimitField.propertyType == SerializedPropertyType.Float)
                        minLimit = minLimitField.floatValue;
                    else if (minLimitField.propertyType == SerializedPropertyType.Integer)
                        minLimit = minLimitField.intValue;
                    else
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                        return;
                    }
                    if (maxLimitField.propertyType == SerializedPropertyType.Float)
                        maxLimit = maxLimitField.floatValue;
                    else if (maxLimitField.propertyType == SerializedPropertyType.Integer)
                        maxLimit = maxLimitField.intValue;
                    else
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                        return;
                    }
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    return;
                }
            }
            float minValue = isVector2 ? property.vector2Value.x : property.vector2IntValue.x;
            float maxValue = isVector2 ? property.vector2Value.y : property.vector2IntValue.y;
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height), label);
            minValue = EditorGUI.FloatField(new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y, 40, position.height), minValue);
            if (minValue < minLimit) minValue = minLimit;
            maxValue = EditorGUI.FloatField(new Rect(position.x + position.width - 40, position.y, 40, position.height), maxValue);
            if (maxValue > maxLimit) maxValue = maxLimit;
            EditorGUI.MinMaxSlider(new Rect(position.x + EditorGUIUtility.labelWidth + 45, position.y, position.width - EditorGUIUtility.labelWidth - 88, position.height), ref minValue, ref maxValue, minLimit, maxLimit);
            if (isVector2) property.vector2Value = new Vector2(minValue, maxValue);
            else property.vector2IntValue = new Vector2Int(Mathf.FloorToInt(minValue), Mathf.FloorToInt(maxValue));
            EditorGUI.EndProperty();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }
}