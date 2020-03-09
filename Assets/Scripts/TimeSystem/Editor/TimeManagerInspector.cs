using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TimeManager))]
public class TimeManagerInspector : SingletonMonoBehaviourInspector
{
    SerializedProperty multiples;
    SerializedProperty UI;
    SerializedProperty timeline;
    SerializedProperty timeSystem;
    SerializedProperty currentMonth;
    SerializedProperty days;
    SerializedProperty weeks;
    SerializedProperty months;
    SerializedProperty years;
    SerializedProperty daysOfYear;

    private void OnEnable()
    {
        multiples = serializedObject.FindProperty("multiples");
        timeline = serializedObject.FindProperty("timeline");
        timeSystem = serializedObject.FindProperty("timeSystem");
        currentMonth = serializedObject.FindProperty("currentMonth");
        days = serializedObject.FindProperty("days");
        weeks = serializedObject.FindProperty("weeks");
        months = serializedObject.FindProperty("months");
        years = serializedObject.FindProperty("years");
        daysOfYear = serializedObject.FindProperty("daysOfYear");
        UI = serializedObject.FindProperty("UI");
    }

    public override void OnInspectorGUI()
    {
        if (!CheckValid(out string text))
        {
            EditorGUILayout.HelpBox(text, MessageType.Error);
            return;
        }
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(UI);
        EditorGUILayout.PropertyField(timeSystem, new GUIContent("时制"));
        EditorGUILayout.PropertyField(multiples, new GUIContent("倍率"));
        if (multiples.floatValue < 0.01666667f) multiples.floatValue = 0.01666667f;
        EditorGUILayout.Slider(timeline, 0, 24, new GUIContent("时间轴"));
        if (!Application.isPlaying) timeline.floatValue %= 24;

        daysOfYear.intValue = EditorGUILayout.IntSlider(new GUIContent("年轴"), daysOfYear.intValue, 1, 360);
        daysOfYear.intValue = daysOfYear.intValue % 360 == 0 ? 360 : daysOfYear.intValue % 360;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("上一周") && TimeManager.Instance.Weeks > 1) daysOfYear.intValue -= 7;
        if (GUILayout.Button("下一周") && TimeManager.Instance.Weeks % 52 != 0) daysOfYear.intValue += 7;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("上一月") && TimeManager.Instance.Months > 1) daysOfYear.intValue -= 30;
        if (GUILayout.Button("下一月") && TimeManager.Instance.Months % 12 != 0) daysOfYear.intValue += 30;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("上一年") && TimeManager.Instance.Years > 1) years.intValue--;
        if (GUILayout.Button("下一年")) years.intValue++;
        EditorGUILayout.EndHorizontal();
        days.intValue = 360 * (years.intValue - 1) + daysOfYear.intValue;

        weeks.intValue = Mathf.CeilToInt(days.intValue * 1.0f / 7);
        months.intValue = Mathf.CeilToInt(days.intValue * 1.0f / 30);

        int monthIndex = Mathf.CeilToInt(daysOfYear.intValue * 1.0f / 30);
        monthIndex = monthIndex % 12 == 0 ? 12 : monthIndex;
        currentMonth.enumValueIndex = monthIndex - 1;

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("时间", TimeManager.Instance.DateString + " " + TimeManager.Instance.Time);
        //EditorGUILayout.PropertyField(days, new GUIContent("天数"));
        //if (days.intValue < 1) days.intValue = 1;
        EditorGUILayout.LabelField("总天数", "第 " + days.intValue + " 天");
        EditorGUILayout.LabelField("总周数", "第 " + weeks.intValue + " 周");
        EditorGUILayout.LabelField("总月数", "第 " + months.intValue + " 月");
        EditorGUILayout.LabelField("总年数", "第 " + years.intValue + " 年");
        EditorGUILayout.LabelField("当月第一天", TimeManager.WeekDayToString(TimeManager.Instance.WeekDayOfTheFirstDayOfCurrentMonth, TimeManager.Instance.TimeSystem));
        EditorGUILayout.LabelField("折合现实总时间(秒)", TimeManager.Instance.TotalTime.ToString());
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        if (Application.isPlaying)
        {
            TimeManager.Instance.UpdateUI();
            CalendarManager.Instance.UpdateUI();
        }
    }
}
