using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZetanStudio.ConditionSystem.Editor
{
    using ZetanStudio.DialogueSystem;

    [CustomPropertyDrawer(typeof(DialogueEvent[]), true)]
    public class ConditionDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Debug.Log("aaaaaa");
            return base.GetPropertyHeight(property, label);
        }
    }
}
