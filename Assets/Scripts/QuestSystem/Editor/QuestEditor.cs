using UnityEditor;
using UnityEngine;

public class QuestEditor : EditorWindow
{
    private Editor editor;
    private Vector2 scrollPos = Vector2.zero;

    public static void CreateWindow(Quest serializedObject)
    {
        QuestEditor window = GetWindow<QuestEditor>("编辑任务");
        window.editor = Editor.CreateEditor(serializedObject);
        window.Show();
    }

    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        editor.OnInspectorGUI();
        GUILayout.EndScrollView();
    }

    private void OnDestroy()
    {
        DestroyImmediate(editor);
    }
}