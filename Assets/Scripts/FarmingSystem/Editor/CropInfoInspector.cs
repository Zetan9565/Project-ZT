using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

[CustomEditor(typeof(CropInformation))]
public class CropInfoInspector : Editor
{
    CropInformation crop;

    SerializedProperty _ID;
    SerializedProperty gatheringInfo;
    SerializedProperty plantSeason;
    SerializedProperty growthTime;
    SerializedProperty timeUnit;
    SerializedProperty repeatTimes;
    SerializedProperty repeatStage;
    SerializedProperty stages;

    ReorderableList stageList;
    float lineHeight;
    float lineHeightSpace;

    private void OnEnable()
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

        crop = target as CropInformation;

        _ID = serializedObject.FindProperty("_ID");
        gatheringInfo = serializedObject.FindProperty("gatheringInfo");
        plantSeason = serializedObject.FindProperty("plantSeason");
        growthTime = serializedObject.FindProperty("growthTime");
        timeUnit = serializedObject.FindProperty("timeUnit");
        repeatTimes = serializedObject.FindProperty("repeatTimes");
        repeatStage = serializedObject.FindProperty("repeatStage");
        stages = serializedObject.FindProperty("stages");
        HandlingStageList();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
        if (string.IsNullOrEmpty(_ID.stringValue) || ExistsID())
        {
            if (!string.IsNullOrEmpty(_ID.stringValue) && ExistsID())
                EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
            else
                EditorGUILayout.HelpBox("识别码为空！", MessageType.Error);
            if (GUILayout.Button("自动生成识别码"))
            {
                _ID.stringValue = GetAutoID();
                EditorGUI.FocusTextInControl(null);
            }
        }
        EditorGUILayout.PropertyField(gatheringInfo, new GUIContent("对应采集物"));
        if (gatheringInfo.objectReferenceValue)
        {
            EditorGUILayout.LabelField("作物名称", (gatheringInfo.objectReferenceValue as GatheringInformation).name);
        }
        EditorGUILayout.PropertyField(plantSeason, new GUIContent("播种季节"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(growthTime, new GUIContent("生长时间"));
        EditorGUILayout.IntPopup(timeUnit, new GUIContent[] { new GUIContent("游戏日"), new GUIContent("游戏月"), new GUIContent("游戏年") }, new int[] { 2, 3, 5 }, new GUIContent(string.Empty));
        int dayMult = 1;
        switch (timeUnit.enumValueIndex)
        {
            case (int)TimeUnit.Month:
                dayMult = 30;
                break;
            case (int)TimeUnit.Year:
                dayMult = 360;
                break;
        }
        if (growthTime.intValue * dayMult < stages.arraySize) growthTime.intValue = stages.arraySize / dayMult;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(stages, new GUIContent("生长阶段"), false);
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        if (stages.isExpanded)
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (stages.arraySize > 4)
            {
                EditorGUILayout.PropertyField(repeatTimes, new GUIContent("可收割次数"));
                if (repeatTimes.intValue < -1) repeatTimes.intValue = -1;
                if (repeatTimes.intValue > 1)
                {
                    EditorGUILayout.IntSlider(repeatStage.FindPropertyRelative("min"), 0, repeatStage.FindPropertyRelative("max").intValue - 1, new GUIContent("收割后重复阶段"));
                    EditorGUILayout.IntSlider(repeatStage.FindPropertyRelative("max"), repeatStage.FindPropertyRelative("min").intValue + 1, stages.arraySize - 1, new GUIContent("到阶段"));
                }
            }
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            stageList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        if (stages.arraySize >= growthTime.intValue * dayMult) stageList.displayAdd = false;
        else stageList.displayAdd = true;
    }

    void HandlingStageList()
    {
        stageList = new ReorderableList(serializedObject, stages, false, true, true, true);

        stageList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            SerializedProperty cropStage = stages.GetArrayElementAtIndex(index);
            SerializedProperty stage = cropStage.FindPropertyRelative("stage");
            SerializedProperty lifespanPer = cropStage.FindPropertyRelative("lifespanPer");
            SerializedProperty min = lifespanPer.FindPropertyRelative("min");
            SerializedProperty max = lifespanPer.FindPropertyRelative("max");
            SerializedProperty graph = cropStage.FindPropertyRelative("graph");
            int lineCount = 1;
            string repeat = string.Empty;
            if (repeatTimes.intValue > 1 && stages.arraySize > 4)
            {
                if (index == repeatStage.FindPropertyRelative("min").intValue) repeat = " ←从此开始";
                if (index == repeatStage.FindPropertyRelative("max").intValue) repeat = " ←到此重复";
            }
            string name = "生长阶段";
            switch (crop.Stages[index].stage)
            {
                case CropStages.Seed:
                    name = "种子期";
                    break;
                case CropStages.Seedling:
                    name = "幼苗期";
                    break;
                case CropStages.Growing:
                    name = "成长期";
                    break;
                case CropStages.Flowering:
                    name = "开花期";
                    break;
                case CropStages.Bearing:
                    name = "结果期";
                    break;
                case CropStages.Maturity:
                    name = "成熟期";
                    break;
                case CropStages.OverMature:
                    name = "过熟期";
                    break;
                case CropStages.Harvested:
                    name = "收割期";
                    break;
                case CropStages.Withered:
                    name = "枯萎期";
                    break;
                case CropStages.Decay:
                    name = "腐朽期";
                    break;
            }
            EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width / 2 - 8, lineHeight), cropStage, new GUIContent(name + repeat));
            EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width - 125, rect.y, 125, lineHeight), index == 0 ? "播种后进入该阶段" : ("第 " + min.intValue + " 天进入该阶段"));
            if (cropStage.isExpanded)
            {
                int from = 1;
                int dayMult = 1;
                switch (timeUnit.enumValueIndex)
                {
                    case (int)TimeUnit.Month:
                        dayMult = 30;
                        break;
                    case (int)TimeUnit.Year:
                        dayMult = 360;
                        break;
                }
                int to = growthTime.intValue * dayMult;
                if (stages.arraySize > 0)
                {
                    if (index == 0)
                    {
                        to = 1;
                    }
                    if (stages.arraySize > 1)
                    {
                        if (index == 0)
                        {
                            to = stages.GetArrayElementAtIndex(index + 1).FindPropertyRelative("lifespanPer").FindPropertyRelative("min").intValue - 1;
                        }
                        else if (index == stages.arraySize - 1)
                        {
                            from = stages.GetArrayElementAtIndex(index - 1).FindPropertyRelative("lifespanPer").FindPropertyRelative("min").intValue + 1;
                        }
                        else if (index > 0 && index < stages.arraySize - 1)
                        {
                            from = stages.GetArrayElementAtIndex(index - 1).FindPropertyRelative("lifespanPer").FindPropertyRelative("min").intValue + 1;
                            to = stages.GetArrayElementAtIndex(index + 1).FindPropertyRelative("lifespanPer").FindPropertyRelative("min").intValue - 1;
                        }
                    }
                }
                max.intValue = to;
                EditorGUI.LabelField(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount, 16, lineHeight), new GUIContent("第"));
                EditorGUI.IntSlider(new Rect(rect.x + 24, rect.y + lineHeightSpace * lineCount, rect.width - 106, lineHeight),
                    min, from, to, new GUIContent(string.Empty));
                EditorGUI.LabelField(new Rect(rect.x + 24 + rect.width - 106, rect.y + lineHeightSpace * lineCount, 80, lineHeight),
                    "天起到第" + (max.intValue + 1) + "天前");
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount, rect.width - 8, lineHeight), stage, new GUIContent("数值对应阶段"));
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount, rect.width - 8, lineHeight), graph, new GUIContent("阶段图形"));
                lineCount++;
            }
            if (graph.objectReferenceValue)
            {
                GUI.enabled = false;
                EditorGUI.ObjectField(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount, lineHeight * 4f - 8, lineHeight * 4f),
                    new GUIContent(string.Empty), graph.objectReferenceValue as Sprite, typeof(Texture2D), false);
                GUI.enabled = true;
            }
        };

        stageList.elementHeightCallback = (index) =>
        {
            float lineMultiples = 1;
            SerializedProperty cropStage = stages.GetArrayElementAtIndex(index);
            if (cropStage.isExpanded)
            {
                lineMultiples += 3;
            }
            if (cropStage.FindPropertyRelative("graph").objectReferenceValue)
                lineMultiples += 3f;
            return lineHeightSpace * lineMultiples;
        };

        stageList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            if (stages.arraySize == 0)
            {
                crop.Stages.Add(new CropStage());
            }
            else if (stages.arraySize > 0)
            {
                crop.Stages.Add(new CropStage(stages.GetArrayElementAtIndex(stages.arraySize - 1).FindPropertyRelative("lifespanPer").FindPropertyRelative("min").intValue + 1));
            }
        };

        stageList.drawHeaderCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "阶段列表");
        };

        stageList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    string GetAutoID()
    {
        string newID = string.Empty;
        CropInformation[] crops = Resources.LoadAll<CropInformation>("");
        for (int i = 1; i < 1000; i++)
        {
            newID = "CROP" + i.ToString().PadLeft(3, '0');
            if (!Array.Exists(crops, x => x.ID == newID))
                break;
        }
        return newID;
    }

    bool ExistsID()
    {
        CropInformation[] crops = Resources.LoadAll<CropInformation>("");

        CropInformation find = Array.Find(crops, x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != crop || (find == crop && Array.FindAll(crops, x => x.ID == _ID.stringValue).Length > 1);
    }
}
