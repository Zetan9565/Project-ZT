using UnityEngine;
using UnityEditor;
using ZetanStudio.TimeSystem;

[CustomEditor(typeof(TimeManager))]
public class TimeManagerInspector : SingletonMonoBehaviourInspector
{
    SerializedProperty multiples;
    SerializedProperty UI;
    SerializedProperty timeline;
    SerializedProperty timeSystem;

    TimeManager manager;
    int dayOfYear;

    private void OnEnable()
    {
        manager = target as TimeManager;
        multiples = serializedObject.FindProperty("multiples");
        timeline = serializedObject.FindProperty("timeline");
        timeSystem = serializedObject.FindProperty("timeSystem");
        UI = serializedObject.FindProperty("UI");
    }

    public override void OnInspectorGUI()
    {
        if (!CheckValid(out string text))
        {
            EditorGUILayout.HelpBox(text, MessageType.Error);
            return;
        }
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(UI);
        EditorGUILayout.PropertyField(timeSystem, new GUIContent("时制"));
        EditorGUILayout.PropertyField(multiples, new GUIContent("倍率"));
        if (multiples.intValue < 1) multiples.intValue = 1;
        manager.Timeline = EditorGUILayout.Slider("时间轴", timeline.floatValue, 0, 24);
        EditorGUILayout.BeginHorizontal();
        dayOfYear = EditorGUILayout.IntSlider("年轴", dayOfYear, 1, 360);
        if (GUILayout.Button("跳至"))
            manager.DayOfYear = dayOfYear;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("天-") && manager.Days > 1)
            manager.TimeStamp -= TimeManager.DayToSeconds / multiples.intValue;
        if (GUILayout.Button("天+"))
            manager.TimeStamp += TimeManager.DayToSeconds / multiples.intValue;
        if (GUILayout.Button("周-") && manager.Weeks > 1)
            manager.TimeStamp -= TimeManager.WeekToSeconds / multiples.intValue;
        if (GUILayout.Button("周+") && manager.Weeks % 52 != 0)
            manager.TimeStamp += TimeManager.WeekToSeconds / multiples.intValue;
        if (GUILayout.Button("月-") && manager.Months > 1)
            manager.TimeStamp -= TimeManager.MonthToSeconds / multiples.intValue;
        if (GUILayout.Button("月+") && manager.Months % 12 != 0)
            manager.TimeStamp += TimeManager.MonthToSeconds / multiples.intValue;
        if (GUILayout.Button("年-") && manager.Years > 1)
            manager.TimeStamp -= TimeManager.YearToSeconds / multiples.intValue;
        if (GUILayout.Button("年+"))
            manager.TimeStamp += TimeManager.YearToSeconds / multiples.intValue;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("时间", manager.DateString + " " + manager.TimeString);
        EditorGUILayout.LabelField("总天数", "第 " + manager.Days + " 天");
        EditorGUILayout.LabelField("总周数", "第 " + manager.Weeks + " 周");
        EditorGUILayout.LabelField("总月数", "第 " + manager.Months + " 月");
        EditorGUILayout.LabelField("总年数", "第 " + manager.Years + " 年");
        EditorGUILayout.LabelField("当月第一天", TimeManager.WeekDayToString(manager.FirstWeekDayOfCurrentMonth, manager.ClockSystem));
        EditorGUILayout.LabelField("折合现实总时间(秒)", manager.TimeStamp.ToString("F0"));
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
