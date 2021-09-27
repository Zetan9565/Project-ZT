using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Interactive)), CanEditMultipleObjects]
public class InteractiveInspector : Editor
{
    SerializedProperty activated;
    SerializedProperty _3D;
    SerializedProperty _name;
    SerializedProperty icon;
    SerializedProperty hidePanelOnInteract;
    SerializedProperty component;
    SerializedProperty interactMethod;
    SerializedProperty interactiveMethod;
    SerializedProperty nameMethod;
    SerializedProperty OnEnter;
    SerializedProperty OnStay;
    SerializedProperty OnExit;
    SerializedProperty OnEnter2D;
    SerializedProperty OnStay2D;
    SerializedProperty OnExit2D;

    private void OnEnable()
    {
        activated = serializedObject.FindProperty("activated");
        _3D = serializedObject.FindProperty("_3D");
        _name = serializedObject.FindProperty("_name");
        icon = serializedObject.FindProperty("icon");
        hidePanelOnInteract = serializedObject.FindProperty("hidePanelOnInteract");
        component = serializedObject.FindProperty("component");
        interactMethod = serializedObject.FindProperty("interactMethod");
        interactiveMethod = serializedObject.FindProperty("interactiveMethod");
        nameMethod = serializedObject.FindProperty("nameMethod");
        OnEnter = serializedObject.FindProperty("OnEnter");
        OnExit = serializedObject.FindProperty("OnExit");
        OnStay = serializedObject.FindProperty("OnStay");
        OnEnter2D = serializedObject.FindProperty("OnEnter2D");
        OnExit2D = serializedObject.FindProperty("OnExit2D");
        OnStay2D = serializedObject.FindProperty("OnStay2D");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(activated, new GUIContent("启用"));
        if(activated.boolValue)
            EditorGUILayout.PropertyField(_3D, new GUIContent("3D"));
        EditorGUILayout.EndHorizontal();
        if (activated.boolValue)
        {
            if (string.IsNullOrEmpty(nameMethod.stringValue))
                EditorGUILayout.PropertyField(_name, new GUIContent("名称"));
            EditorGUILayout.PropertyField(icon, new GUIContent("图标"));
            EditorGUILayout.PropertyField(hidePanelOnInteract, new GUIContent("交互时隐藏面板"));
            EditorGUILayout.PropertyField(component, new GUIContent("搭配组件"));
            if (component.objectReferenceValue)
            {
                var com = component.objectReferenceValue as Component;
                if (com)
                {
                    System.Type type = com.GetType();
                    int index = 0, index2 = 0;
                    var methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                        .Where(x => x.ReturnType.Equals(typeof(bool)) && x.GetParameters().Length == 0).ToArray();
                    string[] meNames = new string[methods.Length + 1];
                    string[] meNames_Com = new string[methods.Length + 1];
                    meNames[0] = "无";
                    meNames_Com[0] = "无";
                    for (int i = 0; i < methods.Length; i++)
                    {
                        var name = methods[i].Name;
                        meNames[i + 1] = name;
                        meNames_Com[i + 1] = $"{name}()";
                        if (name == interactMethod.stringValue)
                            index = i + 1;
                        if (name == interactiveMethod.stringValue)
                            index2 = i + 1;
                    }
                    index = EditorGUILayout.Popup(new GUIContent("交互回调", "返回值是布尔且不含参"), index, meNames_Com);
                    interactMethod.stringValue = index == 0 ? string.Empty : meNames[index];
                    index2 = EditorGUILayout.Popup(new GUIContent("可交互状态回调", "返回值是布尔且不含参"), index2, meNames_Com);
                    interactiveMethod.stringValue = index2 == 0 ? string.Empty : meNames[index2];

                    index = 0;
                    methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                        .Where(x => x.ReturnType.Equals(typeof(string)) && x.GetParameters().Length == 0).ToArray();
                    meNames = new string[methods.Length + 1];
                    meNames_Com = new string[methods.Length + 1];
                    meNames[0] = "无";
                    meNames_Com[0] = "无";
                    for (int i = 0; i < methods.Length; i++)
                    {
                        var name = methods[i].Name;
                        meNames[i + 1] = name;
                        meNames_Com[i + 1] = $"{name}()";
                        if (name == nameMethod.stringValue)
                            index = i + 1;
                    }
                    index = EditorGUILayout.Popup(new GUIContent("名字重写回调", "返回值是字符串且不含参"), index, meNames_Com);
                    nameMethod.stringValue = index == 0 ? string.Empty : meNames[index];
                }
            }
            if (_3D.boolValue)
            {
                EditorGUILayout.PropertyField(OnEnter, new GUIContent("进入可交互范围内时"));
                EditorGUILayout.PropertyField(OnStay, new GUIContent("停留可交互范围内时"));
                EditorGUILayout.PropertyField(OnExit, new GUIContent("离开可交互范围内时"));
            }
            else
            {
                EditorGUILayout.PropertyField(OnEnter2D, new GUIContent("进入可交互范围内时"));
                EditorGUILayout.PropertyField(OnStay2D, new GUIContent("停留可交互范围内时"));
                EditorGUILayout.PropertyField(OnExit2D, new GUIContent("离开可交互范围内时"));
            }
        }
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}