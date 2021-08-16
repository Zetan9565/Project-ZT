using UnityEditor;
using UnityEngine;

public class QuestEditor : EditorWindow
{
    private Editor editor;
    private Vector2 scrollPos = Vector2.zero;

    public static void CreateWindow(Quest serializedObject)
    {
        if (!serializedObject) return;
        QuestEditor window = GetWindow<QuestEditor>("编辑任务");
        window.editor = Editor.CreateEditor(serializedObject);
        (window.editor as QuestInspector).AddAnimaListener(window.Repaint);
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
        if (editor) (editor as QuestInspector).RemoveAnimaListener(Repaint);
        DestroyImmediate(editor);
    }
}