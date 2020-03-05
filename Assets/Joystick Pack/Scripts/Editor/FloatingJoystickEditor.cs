using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FloatingJoystick))]
public class FloatingJoystickEditor : JoystickEditor
{
    private SerializedProperty fade;
    private SerializedProperty fadeInDuration;
    private SerializedProperty fadeOutDuration;
    private SerializedProperty startFadeOutDuration;

    protected override void OnEnable()
    {
        base.OnEnable();
        fade = serializedObject.FindProperty("fade");
        fadeInDuration = serializedObject.FindProperty("fadeInDuration");
        fadeOutDuration = serializedObject.FindProperty("fadeOutDuration");
        startFadeOutDuration = serializedObject.FindProperty("startFadeOutDuration");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(fade);
        EditorGUILayout.PropertyField(fadeInDuration);
        EditorGUILayout.PropertyField(fadeOutDuration);
        EditorGUILayout.PropertyField(startFadeOutDuration);
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();

        if (background != null)
        {
            RectTransform backgroundRect = (RectTransform)background.objectReferenceValue;
            backgroundRect.anchorMax = Vector2.zero;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.pivot = center;
        }
    }
}