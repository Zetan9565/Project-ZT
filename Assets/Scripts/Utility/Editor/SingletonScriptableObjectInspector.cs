using System;
using UnityEditor;

[CustomEditor(typeof(SingletonScriptableObject), true)]
public class SingletonScriptableObjectInspector : Editor
{
    private Type type;

    private void OnEnable()
    {
        type = target.GetType();
    }

    public sealed override void OnInspectorGUI()
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("单例", type.GetField("instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy).GetValue(null) as SingletonScriptableObject, type, false);
        EditorGUI.EndDisabledGroup();
        OnInspectorGUI_();
    }

    protected virtual void OnInspectorGUI_()
    {
        base.OnInspectorGUI();
    }
}
