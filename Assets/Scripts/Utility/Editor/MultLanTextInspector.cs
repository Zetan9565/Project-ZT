using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(MultLangText))]
public class MultLanTextInspector : UnityEditor.UI.TextEditor
{
    //Text text;
    //SerializedProperty ID;
    //SerializedProperty language;

    //protected override void OnEnable()
    //{
    //    base.OnEnable();
    //    text = target as Text;
    //    ID = serializedObject.FindProperty("_ID");
    //    language = serializedObject.FindProperty("language");
    //}

    //public override void OnInspectorGUI()
    //{
    //    serializedObject.UpdateIfRequiredOrScript();
    //    EditorGUI.BeginChangeCheck();
    //    EditorGUILayout.PropertyField(ID);
    //    EditorGUILayout.PropertyField(language);
    //    if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    //    base.OnInspectorGUI();
    //}
}