using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Window), true)]
public class WindowInspector : Editor
{
    protected Window window;
    protected SerializedProperty animated;
    protected SerializedProperty duration;
    protected SerializedProperty content;
    protected SerializedProperty closeButton;

    private void OnEnable()
    {
        window = target as Window;
        animated = serializedObject.FindProperty("animated");
        duration = serializedObject.FindProperty("duration");
        content = serializedObject.FindProperty("content");
        closeButton = serializedObject.FindProperty("closeButton");
        EnableOther();
    }
    protected virtual void EnableOther() { }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        if (Application.isPlaying)
        {
            if (GUILayout.Button("打开"))
                window.Open();
            if (GUILayout.Button("关闭"))
                window.Close();
            if (window is IHideable hideable)
                if (GUILayout.Button("显隐"))
                    WindowsManager.HideWindow(hideable, !hideable.IsHidden);
        }
        else
        {
            if (content.objectReferenceValue is CanvasGroup group)
            {
                if (GUILayout.Button("显示"))
                {
                    group.alpha = 1;
                    group.blocksRaycasts = true;
                }
                if (GUILayout.Button("隐藏"))
                {
                    group.alpha = 0;
                    group.blocksRaycasts = false;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        Rect rect = EditorGUILayout.GetControlRect();
        EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width / 2 - 1, rect.height), animated);
        if (animated.boolValue) EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2 + 1, rect.y, rect.width - 1, rect.height), duration);
        EditorGUILayout.PropertyField(content);
        EditorGUILayout.PropertyField(closeButton);
        InspectOther();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }

    protected virtual void InspectOther()
    {
        SerializedProperty temp = closeButton.GetEndProperty();
        if (temp != null && !string.IsNullOrEmpty(temp.propertyPath))
        {
            EditorGUILayout.PropertyField(temp, true);
            bool enterChildren = true;
            while (temp.NextVisible(enterChildren))
            {
                EditorGUILayout.PropertyField(temp, true);
                enterChildren = false;
            }
        }
    }
}