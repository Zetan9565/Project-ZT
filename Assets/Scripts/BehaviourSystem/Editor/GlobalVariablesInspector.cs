using UnityEditor;

namespace ZetanStudio.BehaviourTree.Editor
{
    [CustomEditor(typeof(GlobalVariables))]
    public class GlobalVariablesInspector : UnityEditor.Editor
    {
        SerializedProperty variables;
        SharedVariableListDrawer variableList;

        private void OnEnable()
        {
            variables = serializedObject.FindProperty("variables");
            variableList = new SharedVariableListDrawer(variables, false);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            variableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}