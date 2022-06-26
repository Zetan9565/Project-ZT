using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProductInformation))]
public class ProductInfoInspector : Editor
{
    SerializedProperty remark;
    SerializedProperty products;

    private void OnEnable()
    {
        remark = serializedObject.FindProperty("remark");
        products = serializedObject.FindProperty("products");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(remark, new GUIContent("备注"));
        EditorGUILayout.PropertyField(products, new GUIContent("产出"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}