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
    SerializedProperty _name;
    SerializedProperty cropType;
    SerializedProperty temperature;
    SerializedProperty humidity;
    SerializedProperty size;
    SerializedProperty prefab;
    SerializedProperty previewPrefab;
    SerializedProperty plantSeason;
    SerializedProperty stages;

    ReorderableList stageList;
    float lineHeight;
    float lineHeightSpace;

    private void OnEnable()
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 2;

        crop = target as CropInformation;

        _ID = serializedObject.FindProperty("_ID");
        _name = serializedObject.FindProperty("_name");
        cropType = serializedObject.FindProperty("cropType");
        temperature = serializedObject.FindProperty("temperature");
        humidity = serializedObject.FindProperty("humidity");
        size = serializedObject.FindProperty("size");
        prefab = serializedObject.FindProperty("prefab");
        previewPrefab = serializedObject.FindProperty("previewPrefab");
        plantSeason = serializedObject.FindProperty("plantSeason");
        stages = serializedObject.FindProperty("stages");
        HandlingStageList();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
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
        EditorGUILayout.PropertyField(_name, new GUIContent("作物名称"));
        EditorGUILayout.PropertyField(cropType, new GUIContent("作物类型"));
        EditorGUILayout.PropertyField(temperature, new GUIContent("生长温度"));
        EditorGUILayout.PropertyField(humidity, new GUIContent("生长湿度"));
        EditorGUILayout.PropertyField(size, new GUIContent("占用空间"));
        EditorGUILayout.PropertyField(prefab, new GUIContent("预制件"));
        EditorGUILayout.PropertyField(previewPrefab, new GUIContent("预览预制件"));
        EditorGUILayout.PropertyField(plantSeason, new GUIContent("播种季节"));
        EditorGUILayout.LabelField("生长周期", crop.Lifespan + "天");
        EditorGUILayout.PropertyField(stages, new GUIContent("生长阶段"), false);
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        if (stages.isExpanded)
        {
            serializedObject.UpdateIfRequiredOrScript();
            stageList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        if (stages.arraySize >= 10) stageList.displayAdd = false;
        else stageList.displayAdd = true;
    }

    void HandlingStageList()
    {
        stageList = new ReorderableList(serializedObject, stages, false, true, true, true);

        stageList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            SerializedProperty cropStage = stages.GetArrayElementAtIndex(index);
            SerializedProperty lastingDays = cropStage.FindPropertyRelative("lastingDays");
            SerializedProperty repeatTimes = cropStage.FindPropertyRelative("repeatTimes");
            SerializedProperty indexToReturn = cropStage.FindPropertyRelative("indexToReturn");
            SerializedProperty gatherInfo = cropStage.FindPropertyRelative("gatherInfo");
            SerializedProperty graph = cropStage.FindPropertyRelative("graph");
            string name = "[阶段" + index + "]";
            switch (crop.Stages[index].Stage)
            {
                case CropStageType.Seed:
                    name += "种子期";
                    break;
                case CropStageType.Seedling:
                    name += "幼苗期";
                    break;
                case CropStageType.Growing:
                    name += "成长期";
                    break;
                case CropStageType.Flowering:
                    name += "开花期";
                    break;
                case CropStageType.Bearing:
                    name += "结果期";
                    break;
                case CropStageType.Maturity:
                    name += "成熟期";
                    break;
                case CropStageType.OverMature:
                    name += "过熟期";
                    break;
                case CropStageType.Harvested:
                    name += "收割期";
                    break;
                case CropStageType.Withered:
                    name += "枯萎期";
                    break;
                case CropStageType.Decay:
                    name += "腐朽期";
                    break;
            }
            EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width / 4 - 8, lineHeight), cropStage, new GUIContent(name));
            EditorGUI.LabelField(new Rect(rect.x + rect.width - 166, rect.y, 30, lineHeight), "持续");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width - 138, rect.y, 26, lineHeight), lastingDays, new GUIContent(string.Empty));
            if (lastingDays.intValue < 1)
            {
                if (index == stages.arraySize - 1)
                    lastingDays.intValue = -1;
                else lastingDays.intValue = 1;
            }
            EditorGUI.LabelField(new Rect(rect.x + rect.width - 110, rect.y, 16, lineHeight), "天");
            EditorGUI.LabelField(new Rect(rect.x + rect.width - 86, rect.y, 40, lineHeight), "可收割");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width - 46, rect.y, 26, lineHeight), repeatTimes, new GUIContent(string.Empty));
            if (repeatTimes.intValue < 0) repeatTimes.intValue = -1;
            EditorGUI.LabelField(new Rect(rect.x + rect.width - 18, rect.y, 16, lineHeight), "次");
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            if (cropStage.isExpanded)
            {
                int lineCount = 1;
                serializedObject.UpdateIfRequiredOrScript();
                EditorGUI.BeginChangeCheck();
                graph.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x - rect.width + lineHeight * 4.2f, rect.y + lineHeightSpace * lineCount, rect.width - 8, lineHeight * 3.2f),
                    string.Empty, graph.objectReferenceValue, typeof(Sprite), false);
                if (repeatTimes.intValue != 0)
                {
                    if (!gatherInfo.objectReferenceValue || gatherInfo.objectReferenceValue && !AssetDatabase.IsSubAsset(gatherInfo.objectReferenceValue))
                    {
                        EditorGUI.PropertyField(new Rect(rect.x - 4 + lineHeight * 4.5f, rect.y + lineHeightSpace * lineCount, rect.width - lineHeight * 4f, lineHeight), gatherInfo, new GUIContent("对应采集物信息"));
                        lineCount++;
                    }
                    else if (AssetDatabase.IsSubAsset(gatherInfo.objectReferenceValue))
                    {
                        EditorGUI.LabelField(new Rect(rect.x - 4 + lineHeight * 4.5f, rect.y + lineHeightSpace * lineCount, rect.width - lineHeight * 4f, lineHeight),
                            $"对应采集物信息{((gatherInfo.objectReferenceValue as ResourceInformation).IsValid ? string.Empty : "(未补全)")}");
                        lineCount++;
                    }
                    if (gatherInfo.objectReferenceValue)
                    {
                        if (GUI.Button(new Rect(rect.x - 4 + lineHeight * 4.5f, rect.y + lineHeightSpace * lineCount, rect.width - lineHeight * 4f, lineHeight), "编辑"))
                            EditorUtility.OpenPropertyEditor(gatherInfo.objectReferenceValue);
                        lineCount++;
                        if (AssetDatabase.IsSubAsset(gatherInfo.objectReferenceValue))
                        {
                            if (GUI.Button(new Rect(rect.x - 4 + lineHeight * 4.5f, rect.y + lineHeightSpace * lineCount, rect.width - lineHeight * 4f, lineHeight), "删除"))
                            {
                                AssetDatabase.RemoveObjectFromAsset(gatherInfo.objectReferenceValue);
                                gatherInfo.objectReferenceValue = null;
                                AssetDatabase.SaveAssets();
                            }
                            lineCount++;
                        }
                    }
                    else if (GUI.Button(new Rect(rect.x - 4 + lineHeight * 4.5f, rect.y + lineHeightSpace * lineCount, rect.width - lineHeight * 4f, lineHeight), "新建"))
                    {
                        ResourceInformation infoInstance = CreateInstance<ResourceInformation>();
                        infoInstance.SetBaseName("resource info");
                        AssetDatabase.AddObjectToAsset(infoInstance, target);
                        AssetDatabase.SaveAssets();

                        gatherInfo.objectReferenceValue = infoInstance;
                        SerializedObject gInfoObj = new SerializedObject(gatherInfo.objectReferenceValue);
                        SerializedProperty _ID = gInfoObj.FindProperty("_ID");
                        SerializedProperty _name = gInfoObj.FindProperty("_name");
                        _ID.stringValue = this._ID.stringValue + "S" + index;
                        _name.stringValue = this._name.stringValue;
                        gInfoObj.ApplyModifiedProperties();

                        EditorUtility.OpenPropertyEditor(infoInstance);
                    }
                    lineCount++;
                }
                if ((repeatTimes.intValue < 0 || repeatTimes.intValue > 1) && index > 0)
                {
                    lineCount = 4;
                    EditorGUI.IntSlider(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount, rect.width - 8, lineHeight),
                        indexToReturn, 0, index - 1, new GUIContent("收割后返回阶段"));
                }
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            }
        };

        stageList.elementHeightCallback = (index) =>
        {
            int lineCount = 1;
            float listHeight = 0;
            SerializedProperty cropStage = stages.GetArrayElementAtIndex(index);
            SerializedProperty gatherInfo = cropStage.FindPropertyRelative("gatherInfo");
            SerializedProperty repeatTimes = cropStage.FindPropertyRelative("repeatTimes");
            if (cropStage.isExpanded)
            {
                lineCount += 3;//空白
                if ((repeatTimes.intValue < 0 || repeatTimes.intValue > 1) && index > 0)
                {
                    lineCount++;
                }
            }
            return lineHeightSpace * lineCount + listHeight;
        };

        stageList.onRemoveCallback = (list) =>
        {
            if (crop.Stages.Count < 10)
            {
                serializedObject.UpdateIfRequiredOrScript();
                EditorGUI.BeginChangeCheck();
                if (EditorUtility.DisplayDialog("删除", "确定删除这个阶段吗？", "确定", "取消"))
                    serializedObject.FindProperty("stages").DeleteArrayElementAtIndex(list.index);
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            }
        };

        stageList.onAddDropdownCallback = (_rect, _list) =>
        {
            GenericMenu menu = new GenericMenu();
            if (!crop.Stages.Exists(s => s.Stage == CropStageType.Seed)) menu.AddItem(new GUIContent("种子期"), false, OnAddOption, CropStageType.Seed);
            if (!crop.Stages.Exists(s => s.Stage == CropStageType.Seedling)) menu.AddItem(new GUIContent("幼苗期"), false, OnAddOption, CropStageType.Seedling);
            if (!crop.Stages.Exists(s => s.Stage == CropStageType.Growing)) menu.AddItem(new GUIContent("成长期"), false, OnAddOption, CropStageType.Growing);
            if (!crop.Stages.Exists(s => s.Stage == CropStageType.Flowering)) menu.AddItem(new GUIContent("开花期"), false, OnAddOption, CropStageType.Flowering);
            if (!crop.Stages.Exists(s => s.Stage == CropStageType.Bearing)) menu.AddItem(new GUIContent("结果期"), false, OnAddOption, CropStageType.Bearing);
            if (!crop.Stages.Exists(s => s.Stage == CropStageType.Maturity)) menu.AddItem(new GUIContent("成熟期"), false, OnAddOption, CropStageType.Maturity);
            if (!crop.Stages.Exists(s => s.Stage == CropStageType.OverMature)) menu.AddItem(new GUIContent("过熟期"), false, OnAddOption, CropStageType.OverMature);
            if (!crop.Stages.Exists(s => s.Stage == CropStageType.Harvested)) menu.AddItem(new GUIContent("收割期"), false, OnAddOption, CropStageType.Harvested);
            if (!crop.Stages.Exists(s => s.Stage == CropStageType.Withered)) menu.AddItem(new GUIContent("枯萎期"), false, OnAddOption, CropStageType.Withered);
            if (!crop.Stages.Exists(s => s.Stage == CropStageType.Decay)) menu.AddItem(new GUIContent("腐朽期"), false, OnAddOption, CropStageType.Decay);
            menu.DropDown(_rect);

            void OnAddOption(object data)
            {
                var cropStage = (CropStageType)data;
                crop.Stages.Add(new CropStage(1, cropStage));
                Dictionary<CropStage, CropStage> returnStages = new Dictionary<CropStage, CropStage>();
                for (int i = 0; i < crop.Stages.Count; i++)
                    returnStages.Add(crop.Stages[i], crop.Stages[crop.Stages[i].IndexToReturn]);
                crop.Stages.Sort((x, y) =>
                {
                    if (x.Stage < y.Stage) return -1;
                    else if (x.Stage > y.Stage) return 1;
                    else return 0;
                });
                serializedObject.UpdateIfRequiredOrScript();
                EditorGUI.BeginChangeCheck();
                foreach (var returnStage in returnStages)
                {
                    SerializedProperty stage = stages.GetArrayElementAtIndex(crop.Stages.IndexOf(returnStage.Key));
                    stage.FindPropertyRelative("indexToReturn").intValue = crop.Stages.IndexOf(returnStage.Value);
                }
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
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
        CropInformation[] crops = Resources.LoadAll<CropInformation>("Configuration");
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
        CropInformation[] crops = Resources.LoadAll<CropInformation>("Configuration");

        CropInformation find = Array.Find(crops, x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != crop || (find == crop && Array.FindAll(crops, x => x.ID == _ID.stringValue).Length > 1);
    }
}
