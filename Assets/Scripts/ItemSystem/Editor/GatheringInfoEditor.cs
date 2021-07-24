using UnityEditor;

public class GatheringInfoEditor : EditorWindow
{
    private Editor editor;

    public static void CreateWindow(GatheringInformation serializedObject)
    {
        GatheringInfoEditor window = GetWindow<GatheringInfoEditor>("编辑采集物");
        window.editor = Editor.CreateEditor(serializedObject);
        window.Show();
    }

    private void OnGUI()
    {
        editor.OnInspectorGUI();
    }
}