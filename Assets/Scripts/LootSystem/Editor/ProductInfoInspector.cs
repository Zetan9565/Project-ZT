using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProductInformation))]
public class ProductInfoInspector : Editor
{
    SerializedProperty remark;
    SerializedProperty products;
    DropItemListDrawer listDrawer;

    private void OnEnable()
    {
        remark = serializedObject.FindProperty("remark");
        products = serializedObject.FindProperty("products");
        listDrawer = new DropItemListDrawer(serializedObject, products, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight + 2);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(remark, new GUIContent("备注"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

        serializedObject.UpdateIfRequiredOrScript();
        listDrawer.DoLayoutDraw();
        serializedObject.ApplyModifiedProperties();
    }
}