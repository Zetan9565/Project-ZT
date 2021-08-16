using UnityEditor;
using UnityEngine;

public class FormulationEditor : EditorWindow
{
    private Editor editor;
    private Vector2 scrollPos;

    public static void CreateWindow(Formulation serializedObject)
    {
        if (!serializedObject) return;
        FormulationEditor window = GetWindow<FormulationEditor>();
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