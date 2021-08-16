using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(Story))]
public class StoryInspector : Editor
{
    Story story;

    ReorderableList plotList;
    Dictionary<Plot, ReorderableList> plotActionLists = new Dictionary<Plot, ReorderableList>();

    SerializedProperty plots;


    float lineHeight;
    float lineHeightSpace;

    private void OnEnable()
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 2;

        story = target as Story;

        plots = serializedObject.FindProperty("plots");

        HandlingPlotList();
    }

    public override void OnInspectorGUI()
    {
        if (!CheckEditComplete())
            EditorGUILayout.HelpBox("该剧情存在未补全信息。", MessageType.Warning);
        else
            EditorGUILayout.HelpBox("该剧情信息已完整。", MessageType.Info);
        serializedObject.Update();
        plotList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    void HandlingPlotList()
    {
        plotList = new ReorderableList(serializedObject, plots, true, true, true, true);

        plotList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            SerializedProperty plot = plots.GetArrayElementAtIndex(index);
            SerializedProperty remark = plot.FindPropertyRelative("remark");
            SerializedProperty actions = plot.FindPropertyRelative("actions");
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (!string.IsNullOrEmpty(remark.stringValue))
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), remark.stringValue);
            else EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "无备注情节-" + (index + 1).ToString().PadLeft(2, '0'));
            int lineCount = 1;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), remark, new GUIContent("备注"));
            lineCount++;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), actions, new GUIContent("行为"));
            lineCount++;
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            if (actions.isExpanded)
            {
                ReorderableList actionsList;
                if (!plotActionLists.ContainsKey(story.Plots[index]))
                {
                    actionsList = new ReorderableList(plot.serializedObject, actions, true, true, true, true);
                    plotActionLists.Add(story.Plots[index], actionsList);
                    actionsList.drawElementCallback = (rect2, index2, isActive2, isFocused2) =>
                    {
                        plot.serializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        SerializedProperty action = actions.GetArrayElementAtIndex(index2);
                        SerializedProperty actionType = action.FindPropertyRelative("actionType");
                        SerializedProperty dialogue = action.FindPropertyRelative("dialogue");
                        SerializedProperty forPlayer = action.FindPropertyRelative("forPlayer");
                        SerializedProperty animaActionType = action.FindPropertyRelative("animaActionType");
                        SerializedProperty character = action.FindPropertyRelative("character");
                        SerializedProperty paramName = action.FindPropertyRelative("paramName");
                        SerializedProperty intValue = action.FindPropertyRelative("intValue");
                        SerializedProperty boolValue = action.FindPropertyRelative("boolValue");
                        SerializedProperty floatValue = action.FindPropertyRelative("floatValue");
                        SerializedProperty animaClip = action.FindPropertyRelative("animaClip");
                        SerializedProperty direction = action.FindPropertyRelative("direction");
                        SerializedProperty distance = action.FindPropertyRelative("distance");
                        SerializedProperty duration = action.FindPropertyRelative("duration");
                        SerializedProperty zoomMultiple = action.FindPropertyRelative("zoomMultiple");
                        SerializedProperty extent = action.FindPropertyRelative("extent");
                        SerializedProperty frequency = action.FindPropertyRelative("frequency");
                        string label = string.Empty;
                        switch (story.Plots[index].Actions[index2].ActionType)
                        {
                            case PlotActionType.Animation: label = "角色动画"; break;
                            case PlotActionType.Dialogue: label = "对话"; break;
                            case PlotActionType.FlashScreen: label = "屏幕闪烁"; break;
                            case PlotActionType.ShakeCamera: label = "相机抖动"; break;
                            case PlotActionType.TransferCamera: label = "相机移动"; break;
                            case PlotActionType.TransferCharacter: label = "角色移动"; break;
                            case PlotActionType.WaitForSecond: label = "稍等"; break;
                            case PlotActionType.ZoomScreen: label = "闪烁缩放"; break;
                            default: label = "(空行为)"; break;
                        }
                        if (label != "(空行为)") EditorGUI.PropertyField(new Rect(rect2.x + 8, rect2.y, rect2.width / 2, lineHeight), action, new GUIContent(label));
                        else EditorGUI.LabelField(new Rect(rect2.x, rect2.y, rect2.width / 2, lineHeight), label);
                        EditorGUI.PropertyField(new Rect(rect2.x + rect2.width / 2, rect2.y, rect2.width / 2, lineHeight), actionType, new GUIContent(string.Empty));
                        int _lineCount = 1;
                        if (action.isExpanded)
                        {
                            switch (actionType.enumValueIndex)
                            {
                                case (int)PlotActionType.Dialogue:
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        dialogue, new GUIContent("对话"));
                                    _lineCount++;
                                    if (dialogue.objectReferenceValue && (dialogue.objectReferenceValue as Dialogue).Words.Count > 0 && (dialogue.objectReferenceValue as Dialogue).Words[0] != null)
                                    {
                                        GUI.enabled = false;
                                        EditorGUI.TextField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                            (dialogue.objectReferenceValue as Dialogue).Words[0].Content);
                                        GUI.enabled = true;
                                        _lineCount++;
                                    }
                                    break;
                                case (int)PlotActionType.TransferCharacter:
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        forPlayer, new GUIContent("控制玩家"));
                                    _lineCount++;
                                    if (!forPlayer.boolValue)
                                    {
                                        EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                            character, new GUIContent("所控制角色"));
                                        _lineCount++;
                                        if (character.objectReferenceValue)
                                        {
                                            EditorGUI.LabelField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                                "角色名字", (character.objectReferenceValue as CharacterInformation).name);
                                            _lineCount++;
                                        }
                                    }
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        direction, new GUIContent("移动方向"));
                                    _lineCount++;
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        distance, new GUIContent("移动距离"));
                                    _lineCount++;
                                    if (distance.floatValue < 0) distance.floatValue = 0;
                                    break;
                                case (int)PlotActionType.Animation:
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        animaActionType, new GUIContent("动画行为类型"));
                                    _lineCount++;
                                    switch (animaActionType.enumValueIndex)
                                    {
                                        case (int)PlotAnimationType.SetInt:
                                            EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                                paramName, new GUIContent("参数名称"));
                                            _lineCount++;
                                            EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                                intValue, new GUIContent("值"));
                                            _lineCount++;
                                            break;
                                        case (int)PlotAnimationType.SetBool:
                                            EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                                paramName, new GUIContent("参数名称"));
                                            _lineCount++;
                                            EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                                boolValue, new GUIContent("值"));
                                            _lineCount++;
                                            break;
                                        case (int)PlotAnimationType.SetFloat:
                                            EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                                paramName, new GUIContent("参数名称"));
                                            _lineCount++;
                                            EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                                floatValue, new GUIContent("值"));
                                            _lineCount++;
                                            break;
                                        case (int)PlotAnimationType.SetTrigger:
                                            EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                                paramName, new GUIContent("参数名称"));
                                            _lineCount++;
                                            break;
                                        case (int)PlotAnimationType.PlayClip:
                                            EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                                animaClip, new GUIContent("动画片段"));
                                            _lineCount++;
                                            break;
                                        default: break;
                                    }
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        forPlayer, new GUIContent("控制玩家"));
                                    _lineCount++;
                                    if (!forPlayer.boolValue)
                                    {
                                        EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                            character, new GUIContent("所控制角色"));
                                        _lineCount++;
                                        if (character.objectReferenceValue)
                                        {
                                            EditorGUI.LabelField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                                "角色名字", (character.objectReferenceValue as CharacterInformation).name);
                                            _lineCount++;
                                        }
                                    }
                                    break;
                                case (int)PlotActionType.WaitForSecond:
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        duration, new GUIContent("等待时间"));
                                    _lineCount++;
                                    break;
                                case (int)PlotActionType.TransferCamera:
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        direction, new GUIContent("移动方向"));
                                    _lineCount++;
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        distance, new GUIContent("移动距离"));
                                    _lineCount++;
                                    if (distance.floatValue < 0) distance.floatValue = 0;
                                    break;
                                case (int)PlotActionType.ShakeCamera:
                                case (int)PlotActionType.FlashScreen:
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        extent, new GUIContent("幅度"));
                                    if (extent.intValue < 0) extent.intValue = 0;
                                    _lineCount++;
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        frequency, new GUIContent("频率"));
                                    if (frequency.intValue < 0) frequency.intValue = 0;
                                    _lineCount++;
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        duration, new GUIContent("持续时间"));
                                    if (duration.floatValue < 0) duration.floatValue = 0;
                                    _lineCount++;
                                    break;
                                case (int)PlotActionType.ZoomScreen:
                                    EditorGUI.PropertyField(new Rect(rect2.x, rect2.y + lineHeightSpace * _lineCount, rect2.width, lineHeight),
                                        zoomMultiple, new GUIContent("缩放倍数"));
                                    if (zoomMultiple.floatValue < 0) zoomMultiple.floatValue = 0.01f;
                                    _lineCount++;
                                    break;
                                default: break;
                            }
                        }
                        if (EditorGUI.EndChangeCheck())
                            plot.serializedObject.ApplyModifiedProperties();
                    };

                    actionsList.elementHeightCallback = (index2) =>
                    {
                        SerializedProperty action = actions.GetArrayElementAtIndex(index2);
                        SerializedProperty actionType = action.FindPropertyRelative("actionType");
                        SerializedProperty dialogue = action.FindPropertyRelative("dialogue");
                        SerializedProperty forPlayer = action.FindPropertyRelative("forPlayer");
                        SerializedProperty animaActionType = action.FindPropertyRelative("animaActionType");
                        SerializedProperty character = action.FindPropertyRelative("character");
                        int _lineCount = 1;
                        if (action.isExpanded)
                        {
                            switch (actionType.enumValueIndex)
                            {
                                case (int)PlotActionType.Dialogue:
                                    _lineCount++;//对话
                                    if (dialogue.objectReferenceValue)
                                        _lineCount++;//对话第一句
                                    break;
                                case (int)PlotActionType.TransferCharacter:
                                    _lineCount++;//对玩家生效
                                    if (!forPlayer.boolValue)
                                    {
                                        _lineCount++;//生效角色
                                        if (character.objectReferenceValue)
                                            _lineCount++;//角色名字
                                    }
                                    _lineCount += 2;//方向、距离
                                    break;
                                case (int)PlotActionType.Animation:
                                    _lineCount++;//动画行为类型
                                    switch (animaActionType.enumValueIndex)
                                    {
                                        case (int)PlotAnimationType.SetInt:
                                        case (int)PlotAnimationType.SetBool:
                                        case (int)PlotAnimationType.SetFloat:
                                            _lineCount += 2;//参数名、值
                                            break;
                                        case (int)PlotAnimationType.SetTrigger:
                                        case (int)PlotAnimationType.PlayClip:
                                            _lineCount++;//参数名或片段
                                            break;
                                        default: break;
                                    }
                                    _lineCount++;//对玩家生效
                                    if (!forPlayer.boolValue)
                                    {
                                        _lineCount++;//生效角色
                                        if (character.objectReferenceValue)
                                            _lineCount++;//角色名字
                                    }
                                    break;
                                case (int)PlotActionType.WaitForSecond:
                                case (int)PlotActionType.ZoomScreen:
                                    _lineCount++;//等待时间或缩放倍数
                                    break;
                                case (int)PlotActionType.TransferCamera:
                                    _lineCount += 2;//方向、距离或幅度、频率
                                    break;
                                case (int)PlotActionType.ShakeCamera:
                                case (int)PlotActionType.FlashScreen:
                                    _lineCount += 3;//幅度、频率、时长
                                    break;
                                default: break;
                            }
                        }
                        return _lineCount * lineHeightSpace;
                    };

                    actionsList.onAddCallback = (list) =>
                    {
                        serializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        story.Plots[index].Actions.Add(new PlotAction());
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();
                    };

                    actionsList.onRemoveCallback = (list) =>
                    {
                        serializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        if (EditorUtility.DisplayDialog("删除", "确定删除这个行为吗？", "确定", "取消"))
                        {
                            story.Plots[index].Actions.RemoveAt(list.index);
                        }
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();
                    };

                    actionsList.drawHeaderCallback = (rect2) =>
                    {
                        int notCmpltCount = story.Plots[index].Actions.FindAll(act =>
                                act.ActionType == PlotActionType.Dialogue && act.Dialogue == null ||
                                act.ActionType == PlotActionType.Animation && act.AnimaActionType == PlotAnimationType.PlayClip && (act.AnimaClip == null || (!act.ForPlayer && act.Character == null)) ||
                                act.ActionType == PlotActionType.Animation && act.AnimaActionType != PlotAnimationType.PlayClip && string.IsNullOrEmpty(act.ParamName) ||
                                act.ActionType == PlotActionType.TransferCharacter && !act.ForPlayer && act.Character == null).Count;
                        EditorGUI.LabelField(rect2, "行为列表", "数量：" + story.Plots[index].Actions.Count.ToString() +
                            (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
                    };

                    actionsList.drawNoneElementCallback = (rect2) =>
                    {
                        EditorGUI.LabelField(rect2, "空列表");
                    };
                }
                else actionsList = plotActionLists[story.Plots[index]];
                plot.serializedObject.Update();
                actionsList.DoList(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight * (actions.arraySize + 1)));
                plot.serializedObject.ApplyModifiedProperties();
            }
        };

        plotList.elementHeightCallback = (index) =>
        {
            SerializedProperty plot = plots.GetArrayElementAtIndex(index);
            SerializedProperty remark = plot.FindPropertyRelative("remark");
            SerializedProperty actions = plot.FindPropertyRelative("actions");
            int lineCount = 1;
            lineCount += 2;//备注、行为
            float totalListHeight = 0.0f;
            if (actions.isExpanded && plotActionLists.ContainsKey(story.Plots[index]))
            {
                totalListHeight += plotActionLists[story.Plots[index]].GetHeight() + 5;
            }
            return lineCount * lineHeightSpace + totalListHeight;
        };

        plotList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            story.Plots.Add(new Plot());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        plotList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个情节吗？", "确定", "取消"))
            {
                plotActionLists.Remove(story.Plots[list.index]);
                story.Plots.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        plotList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = story.Plots.FindAll(plot => plot.Actions.Exists(act =>
                    act.ActionType == PlotActionType.Dialogue && act.Dialogue == null ||
                    act.ActionType == PlotActionType.Animation && act.AnimaActionType == PlotAnimationType.PlayClip && (act.AnimaClip == null || (!act.ForPlayer && act.Character == null)) ||
                    act.ActionType == PlotActionType.Animation && act.AnimaActionType != PlotAnimationType.PlayClip && string.IsNullOrEmpty(act.ParamName) ||
                    act.ActionType == PlotActionType.TransferCharacter && !act.ForPlayer && act.Character == null)).Count;
            EditorGUI.LabelField(rect, "情节列表", "数量：" + story.Plots.Count.ToString() +
                (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        plotList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    bool CheckEditComplete()
    {
        bool editComplete = true;

        editComplete &= !story.Plots.Exists(plot => plot.Actions.Exists(act =>
             act.ActionType == PlotActionType.Dialogue && act.Dialogue == null ||
             act.ActionType == PlotActionType.Animation && act.AnimaActionType == PlotAnimationType.PlayClip && (act.AnimaClip == null || (!act.ForPlayer && act.Character == null)) ||
             act.ActionType == PlotActionType.Animation && act.AnimaActionType != PlotAnimationType.PlayClip && string.IsNullOrEmpty(act.ParamName) ||
             act.ActionType == PlotActionType.TransferCharacter && !act.ForPlayer && act.Character == null));

        return editComplete;
    }
}
