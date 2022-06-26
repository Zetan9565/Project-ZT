using System.Linq;
using UnityEditor;
using UnityEngine;
using ZetanStudio.Extension;

[CustomEditor(typeof(ScriptableObjectEnum), true)]
public class ScriptableObjectEnumInspector : Editor
{
    SerializedProperty _enum;
    ScriptableObjectEnum obj;

    private void OnEnable()
    {
        _enum = serializedObject.FindProperty("_enum");
        obj = target as ScriptableObjectEnum;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        if (obj.GetEnum().ExistsDuplicate(x => x.Name))
            EditorGUILayout.HelpBox("存在同名枚举值!", MessageType.Error);
        else if (obj.GetEnum().Any(x => string.IsNullOrEmpty(x.Name)))
            EditorGUILayout.HelpBox("存在无名枚举值!", MessageType.Error);
        else EditorGUILayout.HelpBox("无错误", MessageType.Info);
        EditorGUILayout.PropertyField(_enum, new GUIContent("枚举值"));
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}