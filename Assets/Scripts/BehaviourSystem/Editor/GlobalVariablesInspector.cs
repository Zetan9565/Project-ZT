using UnityEditor;

namespace ZetanStudio.BehaviourTree
{
    [CustomEditor(typeof(GlobalVariables))]
    public class GlobalVariablesInspector : Editor
    {
        SerializedProperty variables;
        SharedVariableListDrawer variableList;

        private void OnEnable()
        {
            variables = serializedObject.FindProperty("variables");
            variableList = new SharedVariableListDrawer(serializedObject, variables, false);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            variableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}