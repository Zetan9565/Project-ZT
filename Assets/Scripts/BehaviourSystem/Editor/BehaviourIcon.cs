using UnityEditor;
using UnityEngine;

namespace ZetanStudio.BehaviourTree.Editor
{
    [InitializeOnLoad]
    public class BehaviourIcon
    {
        static BehaviourIcon() { EditorApplication.hierarchyWindowItemOnGUI += EvaluateIcons; }

        private static void EvaluateIcons(int instanceId, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null) return;
            if (go.GetComponent<BehaviourExecutor>() || go.GetComponent<BehaviourManager>())
                DrawIcon(selectionRect);
        }

        private static void DrawIcon(Rect rect)
        {
            Rect r = new Rect(rect.x + rect.width - 16f, rect.y, 16f, 16f);
            GUI.DrawTexture(r, GetTex());
        }

        private static Texture2D GetTex()
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Scripts/BehaviourSystem/Editor/Resources/executor.png");
        }
    }
}