using UnityEditor;

[CustomEditor(typeof(WarehouseAgent))]
public class WarehouseAgentInspector : BuildingInspector
{
    WarehouseAgent agent;

    SerializedProperty warehouse;

    protected override void OnEnable()
    {
        base.OnEnable();
        agent = target as WarehouseAgent;
        warehouse = serializedObject.FindProperty("warehouse");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("识别码", agent.ID);
        SerializedProperty warehouseSize = warehouse.FindPropertyRelative("size");
        warehouseSize.FindPropertyRelative("max").intValue = EditorGUILayout.IntSlider("默认仓库容量(格)",
            warehouseSize.FindPropertyRelative("max").intValue, 30, 150);
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
