using System;
using UnityEditor;

namespace ZetanStudio.Editor
{
    [CustomEditor(typeof(SingletonScriptableObject), true)]
    public class SingletonScriptableObjectInspector : UnityEditor.Editor
    {
        private Type type;

        private void OnEnable()
        {
            type = target.GetType();
        }

        public sealed override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("单例", type.BaseType.GetField("instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null) as SingletonScriptableObject, type, false);
            EditorGUI.EndDisabledGroup();
            var list = Utility.Editor.LoadAssets(type);
            if (list.Count > 1)
            {
                string paths = "存在多个实例：";
                for (int i = 0; i < list.Count; i++)
                {
                    paths += "路径" + (i + 1) + AssetDatabase.GetAssetPath(list[i]);
                }
                EditorGUILayout.HelpBox(paths, MessageType.Error);
            }
            else OnInspectorGUI_();
        }

        protected virtual void OnInspectorGUI_()
        {
            base.OnInspectorGUI();
        }
    }
}