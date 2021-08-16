using UnityEditor;
using UnityEngine;

public class DialogueEditor : EditorWindow
{
    private Editor editor;
    private Vector2 scrollPos = Vector2.zero;

    public static void CreateWindow(Dialogue serializedObject)
    {
        if (!serializedObject) return;
        DialogueEditor window = GetWindow<DialogueEditor>("编辑对话");
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