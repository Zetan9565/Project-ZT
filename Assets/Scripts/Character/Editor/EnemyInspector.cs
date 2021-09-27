using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Enemy))]
public class EnemyInspector : Editor
{
    Enemy enemy;

    SerializedProperty info;

    private void OnEnable()
    {
        enemy = target as Enemy;
        info = serializedObject.FindProperty("info");
    }

    public override void OnInspectorGUI()
    {
        if (enemy.Info)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("敌人名称：" + enemy.Info.Name);
            EditorGUILayout.LabelField("敌人识别码：" + enemy.Info.ID);
            if (enemy.Info.DropItems)
                EditorGUILayout.LabelField("掉落物品数量：" + enemy.Info.DropItems.Products.Count);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("敌人信息为空！", MessageType.Warning);
        }
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(info, new GUIContent("信息"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}