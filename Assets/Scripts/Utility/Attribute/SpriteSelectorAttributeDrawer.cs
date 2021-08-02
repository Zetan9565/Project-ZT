using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class SpriteSelectorAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(SpriteSelectorAttribute))]
public class SpriteSelectorAttributeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 3;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue as Sprite, typeof(Sprite), false);
    }
}
#endif