using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Window), true)]
public partial class WindowInspector : Editor
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
                    NewWindowsManager.HideWindow(hideable, !hideable.IsHidden);
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
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(animated, new GUIContent("淡入淡出"));
        EditorGUILayout.PropertyField(duration, new GUIContent("持续时间"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(content, new GUIContent("窗体"));
        EditorGUILayout.PropertyField(closeButton, new GUIContent("关闭按钮"));
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