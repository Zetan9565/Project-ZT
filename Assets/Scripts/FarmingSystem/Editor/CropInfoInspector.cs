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
    SerializedProperty plantSeason;
    SerializedProperty stages;

    ReorderableList stageList;
    float lineHeight;
    float lineHeightSpace;

    Dictionary<CropStage, ReorderableList> productItemsLists = new Dictionary<CropStage, ReorderableList>();

    private void OnEnable()
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

        crop = target as CropInformation;

        _ID = serializedObject.FindProperty("_ID");
        _name = serializedObject.FindProperty("_name");
        cropType = serializedObject.FindProperty("cropType");
        plantSeason = serializedObject.FindProperty("plantSeason");
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
        EditorGUILayout.PropertyField(_name, new GUIContent("作物名称"));
        EditorGUILayout.PropertyField(cropType, new GUIContent("作物类型"));
        EditorGUILayout.PropertyField(plantSeason, new GUIContent("播种季节"));
        EditorGUILayout.LabelField("生长周期", crop.Lifespan + "天");
        EditorGUILayout.PropertyField(stages, new GUIContent("生长阶段"), false);
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        if (stages.isExpanded)
        {
            serializedObject.Update();
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
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            SerializedProperty cropStage = stages.GetArrayElementAtIndex(index);
            SerializedProperty lastingDays = cropStage.FindPropertyRelative("lastingDays");
            SerializedProperty repeatTimes = cropStage.FindPropertyRelative("repeatTimes");
            SerializedProperty indexToReturn = cropStage.FindPropertyRelative("indexToReturn");
            SerializedProperty gatherType = cropStage.FindPropertyRelative("gatherType");
            SerializedProperty gatherTime = cropStage.FindPropertyRelative("gatherTime");
            SerializedProperty lootPrefab = cropStage.FindPropertyRelative("lootPrefab");
            SerializedProperty productItems = cropStage.FindPropertyRelative("productItems");
            SerializedProperty graph = cropStage.FindPropertyRelative("graph");
            ReorderableList productItemsList = null;
            string name = "[阶段" + index + "]";
            switch (crop.Stages[index].Stage)
            {
                case CropStages.Seed:
                    name += "种子期";
                    break;
                case CropStages.Seedling:
                    name += "幼苗期";
                    break;
                case CropStages.Growing:
                    name += "成长期";
                    break;
                case CropStages.Flowering:
                    name += "开花期";
                    break;
                case CropStages.Bearing:
                    name += "结果期";
                    break;
                case CropStages.Maturity:
                    name += "成熟期";
                    break;
                case CropStages.OverMature:
                    name += "过熟期";
                    break;
                case CropStages.Harvested:
                    name += "收割期";
                    break;
                case CropStages.Withered:
                    name += "枯萎期";
                    break;
                case CropStages.Decay:
                    name += "腐朽期";
                    break;
            }
            EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width / 4 - 8, lineHeight), cropStage, new GUIContent(name));
            EditorGUI.LabelField(new Rect(rect.x + rect.width - 166, rect.y, 30, lineHeight), "持续");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width - 138, rect.y, 26, lineHeight), lastingDays, new GUIContent(string.Empty));
            if (lastingDays.intValue < 1) lastingDays.intValue = 1;
            EditorGUI.LabelField(new Rect(rect.x + rect.width - 110, rect.y, 16, lineHeight), "天");
            EditorGUI.LabelField(new Rect(rect.x + rect.width - 86, rect.y, 40, lineHeight), "可收割");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width - 46, rect.y, 26, lineHeight), repeatTimes, new GUIContent(string.Empty));
            if (repeatTimes.intValue < 0) repeatTimes.intValue = 0;
            EditorGUI.LabelField(new Rect(rect.x + rect.width - 18, rect.y, 16, lineHeight), "次");
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            int lineCount = 1;
            graph.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x - rect.width + lineHeight * 4.5f, rect.y + lineHeightSpace * lineCount, rect.width - 8, lineHeight * 3.5f),
                string.Empty, graph.objectReferenceValue as Sprite, typeof(Sprite), false);
            if (repeatTimes.intValue > 0)
            {
                EditorGUI.PropertyField(new Rect(rect.x - 4 + lineHeight * 4.5f, rect.y + lineHeightSpace * lineCount, rect.width - lineHeight * 4f, lineHeight), gatherType, new GUIContent("采集方式"));
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x - 4 + lineHeight * 4.5f, rect.y + lineHeightSpace * lineCount, rect.width - lineHeight * 4f, lineHeight), gatherTime, new GUIContent("采集耗时(秒)"));
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x - 4 + lineHeight * 4.5f, rect.y + lineHeightSpace * lineCount, rect.width - lineHeight * 4f, lineHeight), lootPrefab, new GUIContent("掉落物预制件"));
                lineCount++;
            }
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            if (cropStage.isExpanded)
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                if (repeatTimes.intValue > 1 && index > 0)
                {
                    EditorGUI.IntSlider(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount, rect.width - 8, lineHeight),
                        indexToReturn, 0, index - 1, new GUIContent("收割后返回阶段"));
                    lineCount++;
                }
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                if (repeatTimes.intValue > 0)
                {
                    serializedObject.Update();
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(new Rect(rect.x + 16, rect.y + lineHeightSpace * lineCount, rect.width - 16, lineHeight), productItems,
                        new GUIContent("产出道具" + (productItems.isExpanded ? string.Empty : ("\t\t" + (productItems.arraySize < 1 ? "无" : "数量:" + productItems.arraySize)))));
                    lineCount++;
                    if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                    if (productItems.isExpanded)
                    {
                        productItemsLists.TryGetValue(crop.Stages[index], out productItemsList);
                        if (productItemsList == null)
                        {
                            productItemsList = new ReorderableList(cropStage.serializedObject, productItems, true, true, true, true);
                            productItemsLists.Add(crop.Stages[index], productItemsList);

                            productItemsList.drawElementCallback = (_rect, _index, _isActive, _isForcus) =>
                            {
                                cropStage.serializedObject.Update();
                                SerializedProperty itemInfo = productItems.GetArrayElementAtIndex(_index);
                                var productItem = crop.Stages[index].ProductItems[_index];
                                if (productItem != null && productItem.Item != null)
                                    EditorGUI.PropertyField(new Rect(_rect.x + 8, _rect.y, _rect.width / 2, lineHeight), itemInfo, new GUIContent(productItem.ItemName));
                                else
                                    EditorGUI.PropertyField(new Rect(_rect.x + 8, _rect.y, _rect.width / 2, lineHeight), itemInfo, new GUIContent("(空)"));
                                EditorGUI.BeginChangeCheck();
                                SerializedProperty item = itemInfo.FindPropertyRelative("item");
                                SerializedProperty amount = itemInfo.FindPropertyRelative("amount");
                                SerializedProperty dropRate = itemInfo.FindPropertyRelative("dropRate");
                                SerializedProperty onlyDropForQuest = itemInfo.FindPropertyRelative("onlyDropForQuest");
                                SerializedProperty binedQuest = itemInfo.FindPropertyRelative("bindedQuest");
                                EditorGUI.PropertyField(new Rect(_rect.x + _rect.width / 2f, _rect.y, _rect.width / 2f, lineHeight),
                                    item, new GUIContent(string.Empty));
                                if (itemInfo.isExpanded)
                                {
                                    int _lineCount = 1;
                                    EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                        amount, new GUIContent("最大掉落数量"));
                                    if (amount.intValue < 1) amount.intValue = 1;
                                    _lineCount++;
                                    EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                        dropRate, new GUIContent("掉落概率百分比"));
                                    if (dropRate.floatValue < 0) dropRate.floatValue = 0.0f;
                                    _lineCount++;
                                    EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                        onlyDropForQuest, new GUIContent("只在进行任务时产出"));
                                    _lineCount++;
                                    if (onlyDropForQuest.boolValue)
                                    {
                                        EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                            binedQuest, new GUIContent("相关任务"));
                                        _lineCount++;
                                        if (binedQuest.objectReferenceValue)
                                        {
                                            EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight), "任务名称",
                                                (binedQuest.objectReferenceValue as Quest).Title);
                                            _lineCount++;
                                        }
                                    }
                                }
                                if (EditorGUI.EndChangeCheck())
                                    cropStage.serializedObject.ApplyModifiedProperties();
                            };

                            productItemsList.elementHeightCallback = (int _index) =>
                            {
                                int _lineCount = 1;
                                if (productItems.GetArrayElementAtIndex(_index).isExpanded)
                                {
                                    _lineCount += 3;//数量、百分比、只在
                                    if (crop.Stages[index].ProductItems[_index].OnlyDropForQuest)
                                    {
                                        _lineCount++;//任务
                                        if (crop.Stages[index].ProductItems[_index].BindedQuest)
                                            _lineCount++;//任务标题
                                    }
                                }
                                return _lineCount * lineHeightSpace;
                            };

                            productItemsList.onAddCallback = (_list) =>
                            {
                                cropStage.serializedObject.Update();
                                EditorGUI.BeginChangeCheck();
                                crop.Stages[index].ProductItems.Add(new DropItemInfo() { Amount = 1, DropRate = 100.0f });
                                if (EditorGUI.EndChangeCheck())
                                    cropStage.serializedObject.ApplyModifiedProperties();
                            };

                            productItemsList.onRemoveCallback = (_list) =>
                            {
                                cropStage.serializedObject.Update();
                                EditorGUI.BeginChangeCheck();
                                if (EditorUtility.DisplayDialog("删除", "确定删除这个产出道具吗？", "确定", "取消"))
                                {
                                    crop.Stages[index].ProductItems.RemoveAt(_list.index);
                                }
                                if (EditorGUI.EndChangeCheck())
                                    cropStage.serializedObject.ApplyModifiedProperties();
                            };

                            productItemsList.drawHeaderCallback = (_rect) =>
                            {
                                int notCmpltCount = crop.Stages[index].ProductItems.FindAll(x => !x.Item).Count;
                                EditorGUI.LabelField(_rect, "产出道具列表", "数量:" + crop.Stages[index].ProductItems.Count + (notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty));
                            };

                            productItemsList.drawNoneElementCallback = (_rect) =>
                            {
                                EditorGUI.LabelField(_rect, "空列表");
                            };
                        }
                        cropStage.serializedObject.Update();
                        productItemsList.DoList(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount, rect.width - 8, lineHeight * (productItems.arraySize + 1)));
                        cropStage.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        };

        stageList.elementHeightCallback = (index) =>
        {
            int lineCount = 1;
            float listHeight = 0;
            SerializedProperty cropStage = stages.GetArrayElementAtIndex(index);
            SerializedProperty productItems = cropStage.FindPropertyRelative("productItems");
            lineCount += 3;
            if (cropStage.isExpanded)
            {
                if (cropStage.FindPropertyRelative("repeatTimes").intValue > 1 && index > 0) lineCount++;
                if (cropStage.FindPropertyRelative("repeatTimes").intValue > 0)
                {
                    lineCount += 1;
                    if (productItems.isExpanded)
                        if (productItemsLists.TryGetValue(crop.Stages[index], out var productItemList))
                            listHeight += productItemList.GetHeight();
                }
            }
            return lineHeightSpace * lineCount + listHeight;
        };

        stageList.onRemoveCallback = (list) =>
        {
            if (crop.Stages.Count < 10)
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                if (EditorUtility.DisplayDialog("删除", "确定删除这个阶段吗？", "确定", "取消"))
                    serializedObject.FindProperty("stages").DeleteArrayElementAtIndex(list.index);
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            }
        };

        stageList.onAddDropdownCallback = (_rect, _list) =>
        {
            GenericMenu menu = new GenericMenu();
            if (!crop.Stages.Exists(s => s.Stage == CropStages.Seed)) menu.AddItem(new GUIContent("种子期"), false, OnAddOption, CropStages.Seed);
            if (!crop.Stages.Exists(s => s.Stage == CropStages.Seedling)) menu.AddItem(new GUIContent("幼苗期"), false, OnAddOption, CropStages.Seedling);
            if (!crop.Stages.Exists(s => s.Stage == CropStages.Growing)) menu.AddItem(new GUIContent("成长期"), false, OnAddOption, CropStages.Growing);
            if (!crop.Stages.Exists(s => s.Stage == CropStages.Flowering)) menu.AddItem(new GUIContent("开花期"), false, OnAddOption, CropStages.Flowering);
            if (!crop.Stages.Exists(s => s.Stage == CropStages.Bearing)) menu.AddItem(new GUIContent("结果期"), false, OnAddOption, CropStages.Bearing);
            if (!crop.Stages.Exists(s => s.Stage == CropStages.Maturity)) menu.AddItem(new GUIContent("成熟期"), false, OnAddOption, CropStages.Maturity);
            if (!crop.Stages.Exists(s => s.Stage == CropStages.OverMature)) menu.AddItem(new GUIContent("过熟期"), false, OnAddOption, CropStages.OverMature);
            if (!crop.Stages.Exists(s => s.Stage == CropStages.Harvested)) menu.AddItem(new GUIContent("收割期"), false, OnAddOption, CropStages.Harvested);
            if (!crop.Stages.Exists(s => s.Stage == CropStages.Withered)) menu.AddItem(new GUIContent("枯萎期"), false, OnAddOption, CropStages.Withered);
            if (!crop.Stages.Exists(s => s.Stage == CropStages.Decay)) menu.AddItem(new GUIContent("腐朽期"), false, OnAddOption, CropStages.Decay);
            menu.DropDown(_rect);

            void OnAddOption(object data)
            {
                var cropStage = (CropStages)data;
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
                serializedObject.Update();
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
