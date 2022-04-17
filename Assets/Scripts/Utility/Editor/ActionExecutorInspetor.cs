using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

[CustomEditor(typeof(ActionExecutor))]
public class ActionExecutorInspetor : Editor
{
    ActionExecutor executor;

    SerializedProperty _ID;
    SerializedProperty endDelayTime;
    SerializedProperty onBegin;
    SerializedProperty onExecuting;
    SerializedProperty onEnd;
    SerializedProperty onUndo;

    private void OnEnable()
    {
        executor = target as ActionExecutor;
        _ID = serializedObject.FindProperty("_ID");
        endDelayTime = serializedObject.FindProperty("endDelayTime");
        onBegin = serializedObject.FindProperty("onBegin");
        onExecuting = serializedObject.FindProperty("onExecuting");
        onEnd = serializedObject.FindProperty("onEnd");
        onUndo = serializedObject.FindProperty("onUndo");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("执行状态", executor.IsExecuting ? (executor.IsDone ? "撤销中" : "执行中") : (executor.IsDone ? "已结束" : "未开始"));
            GUI.enabled = false;
        }
        EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
        if (string.IsNullOrEmpty(_ID.stringValue) || ExistsID() || string.IsNullOrEmpty(Regex.Replace(_ID.stringValue, @"[^0-9]+", "")) || !Regex.IsMatch(_ID.stringValue, @"(\d+)$"))
        {
            if (!string.IsNullOrEmpty(_ID.stringValue) && ExistsID())
                EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
            else if (!string.IsNullOrEmpty(_ID.stringValue) && (string.IsNullOrEmpty(Regex.Replace(_ID.stringValue, @"[^0-9]+", "")) || !Regex.IsMatch(_ID.stringValue, @"(\d+)$")))
            {
                EditorGUILayout.HelpBox("此识别码非法！", MessageType.Error);
            }
            if (GUILayout.Button("自动生成识别码"))
            {
                _ID.stringValue = GetAutoID();
                EditorGUI.FocusTextInControl(null);
            }
        }
        EditorGUILayout.LabelField("结束延时", endDelayTime.floatValue.ToString());
        if (Application.isPlaying) EditorGUILayout.LabelField("已执行时间", executor.ExecutionTime.ToString());
        EditorGUILayout.PropertyField(onBegin, new GUIContent("开始时"));
        EditorGUILayout.PropertyField(onExecuting, new GUIContent("执行时"));
        EditorGUILayout.PropertyField(onEnd, new GUIContent("结束时"));
        EditorGUILayout.PropertyField(onUndo, new GUIContent("撤销时"));
        if (Application.isPlaying) GUI.enabled = true;
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }

    bool ExistsID()
    {
        var executors = FindObjectsOfType<ActionExecutor>();

        ActionExecutor find = System.Array.Find(executors, x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != executor || (find == executor && System.Array.FindAll(executors, x => x.ID == _ID.stringValue).Length > 1);
    }

    string GetAutoID()
    {
        string ID = _ID.stringValue;
        var executors = FindObjectsOfType<ActionExecutor>();
        for (int i = 1; i < 100000; i++)
        {
            ID = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_AEXE" + i.ToString().PadLeft(6, '0');
            if (!System.Array.Exists(executors, x => x.ID == ID)) break;
        }
        return ID;
    }
}