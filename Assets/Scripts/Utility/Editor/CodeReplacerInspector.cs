using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZetanStudio
{
    [CustomEditor(typeof(CodeRreplacer))]
    public class CodeReplacerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("测试"))
            {
                (target as CodeRreplacer).Replace();
            }
        }
    }
}
