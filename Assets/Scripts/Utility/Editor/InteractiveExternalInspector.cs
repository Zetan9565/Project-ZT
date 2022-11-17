using System.Linq;
using UnityEditor;
using UnityEngine;
using ZetanStudio.InteractionSystem;

[CustomEditor(typeof(InteractiveExternalBase), true), CanEditMultipleObjects]
public class InteractiveExternalInspector : Editor
{
    SerializedProperty activated;
    SerializedProperty defaultName;
    SerializedProperty defaultIcon;
    SerializedProperty component;
    SerializedProperty interactMethod;
    SerializedProperty endInteractionMethod;
    SerializedProperty interactableMethod;
    SerializedProperty notInteractableMethod;
    SerializedProperty interactiveMethod;
    SerializedProperty nameMethod;

    private void OnEnable()
    {
        activated = serializedObject.FindProperty("activated");
        defaultName = serializedObject.FindProperty("defaultName");
        defaultIcon = serializedObject.FindProperty("defaultIcon");
        component = serializedObject.FindProperty("component");
        interactMethod = serializedObject.FindProperty("interactMethod");
        endInteractionMethod = serializedObject.FindProperty("endInteractionMethod");
        interactableMethod = serializedObject.FindProperty("interactableMethod");
        notInteractableMethod = serializedObject.FindProperty("notInteractableMethod");
        interactiveMethod = serializedObject.FindProperty("interactiveMethod");
        nameMethod = serializedObject.FindProperty("nameMethod");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(activated, new GUIContent("启用"));
        if (activated.boolValue)
        {
            if (string.IsNullOrEmpty(nameMethod.stringValue))
                EditorGUILayout.PropertyField(defaultName, new GUIContent("默认名字"));
            EditorGUILayout.PropertyField(defaultIcon, new GUIContent("图标"));
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

                    index = 0;
                    int index3 = 0;
                    methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                        .Where(x => x.ReturnType.Equals(typeof(void)) && x.GetParameters().Length == 0).ToArray();
                    meNames = new string[methods.Length + 1];
                    meNames_Com = new string[methods.Length + 1];
                    meNames[0] = "无";
                    meNames_Com[0] = "无";
                    for (int i = 0; i < methods.Length; i++)
                    {
                        var name = methods[i].Name;
                        meNames[i + 1] = name;
                        meNames_Com[i + 1] = $"{name}()";
                        if (name == endInteractionMethod.stringValue)
                            index = i + 1;
                        if (name == interactableMethod.stringValue)
                            index2 = i + 1;
                        if (name == notInteractableMethod.stringValue)
                            index3 = i + 1;
                    }
                    index = EditorGUILayout.Popup(new GUIContent("交互结束时回调", "不含参"), index, meNames_Com);
                    index2 = EditorGUILayout.Popup(new GUIContent("变为可交互时回调", "不含参"), index2, meNames_Com);
                    index3 = EditorGUILayout.Popup(new GUIContent("变为不可交互时回调", "不含参"), index3, meNames_Com);
                    endInteractionMethod.stringValue = index == 0 ? string.Empty : meNames[index];
                    interactableMethod.stringValue = index2 == 0 ? string.Empty : meNames[index2];
                    notInteractableMethod.stringValue = index3 == 0 ? string.Empty : meNames[index3];
                }
            }
        }
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}