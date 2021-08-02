using UnityEditor;
using UnityEngine;

public class GatheringInfoEditor : EditorWindow
{
    private Editor editor;
    private Vector2 scrollPos = Vector2.zero;

    public static void CreateWindow(GatheringInformation serializedObject)
    {
        GatheringInfoEditor window = GetWindow<GatheringInfoEditor>("编辑采集物");
        window.editor = Editor.CreateEditor(serializedObject);
        window.Show();
    }

    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        editor.OnInspectorGUI();
        GUILayout.EndScrollView();
    }
}