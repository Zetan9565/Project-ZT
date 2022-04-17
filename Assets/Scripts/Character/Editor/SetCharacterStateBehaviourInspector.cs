using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SetCharacterStateBehaviour))]
public class SetCharacterStateBehaviourInspector : Editor
{
    SerializedProperty normalizedTime;
    SerializedProperty state;
    SerializedProperty subState;

    SerializedProperty exitNormalizedTime;
    SerializedProperty exitState;
    SerializedProperty exitSubState;

    private void OnEnable()
    {
        normalizedTime = serializedObject.FindProperty("normalizedTime");
        state = serializedObject.FindProperty("state");
        subState = serializedObject.FindProperty("subState");
        exitNormalizedTime = serializedObject.FindProperty("exitNormalizedTime");
        exitState = serializedObject.FindProperty("exitState");
        exitSubState = serializedObject.FindProperty("exitSubState");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(normalizedTime, new GUIContent("进入时间"));
        EditorGUILayout.PropertyField(state, new GUIContent("主状态"));
        DrawSubState(state.enumValueIndex, subState);
        EditorGUILayout.PropertyField(exitNormalizedTime, new GUIContent("退出时间"));
        EditorGUILayout.PropertyField(exitState, new GUIContent("主状态"));
        DrawSubState(exitState.enumValueIndex, exitSubState);
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();

        void DrawSubState(int enumValueIndex, SerializedProperty subState)
        {
            switch ((CharacterStates)enumValueIndex)
            {
                case CharacterStates.Normal:
                    DrawField(typeof(CharacterNormalStates));
                    break;
                case CharacterStates.Abnormal:
                    DrawField(typeof(CharacterAbnormalStates));
                    break;
                case CharacterStates.Gather:
                    DrawField(typeof(CharacterGatherStates));
                    break;
                case CharacterStates.Attack:
                    DrawField(typeof(CharacterAttackStates));
                    break;
                case CharacterStates.Busy:
                    DrawField(typeof(CharacterBusyStates));
                    break;
                default:
                    break;
            }

            void DrawField(Type enumType)
            {
                List<int> values = new List<int>();
                List<string> names = new List<string>();
                foreach (var value in Enum.GetValues(enumType))
                {
                    values.Add((int)value);
                    names.Add(ZetanUtility.GetInspectorName((Enum)value));
                }
                int min = values.Min();
                if (subState.intValue < min) subState.intValue = min;
                int max = values.Max();
                if (subState.intValue > max) subState.intValue = max;
                subState.intValue = EditorGUILayout.IntPopup("子状态", subState.intValue, names.ToArray(), values.ToArray());
            }
        }
    }
}