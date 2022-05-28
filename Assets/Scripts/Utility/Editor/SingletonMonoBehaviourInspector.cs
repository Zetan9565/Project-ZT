using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SingletonMonoBehaviour), true)]
public class SingletonMonoBehaviourInspector : Editor
{
    [InitializeOnLoadMethod]
    private static void AddListener()
    {
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged()
    {
        var g = FindObjectsOfType<SingletonMonoBehaviour>().GroupBy(x => x.GetType()).FirstOrDefault(g => g.Count() > 1);
        if (g != null) Debug.LogError(string.Format("存在多个激活的{0}，请确保只激活一个", g.Key.Name));
    }

    public override void OnInspectorGUI()
    {
        if (!CheckValid(out string text)) EditorGUILayout.HelpBox(text, MessageType.Error);
        else base.OnInspectorGUI();
    }

    protected bool CheckValid(out string text)
    {
        var monos = FindObjectsOfType(target.GetType());
        if (monos.Count(x => (x as MonoBehaviour).isActiveAndEnabled) > 1)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("存在多个激活的<");
            sb.Append(target.GetType().Name); sb.Append(">，请移除或失活其它\n");
            for (int i = 0; i < monos.Length; i++)
            {
                sb.Append("位置"); sb.Append(i + 1); sb.Append(": ");
                MonoBehaviour mono = monos[i] as MonoBehaviour;
                List<Transform> parents = new List<Transform>();
                Transform parent = mono.transform.parent;
                while (parent)
                {
                    parents.Add(parent);
                    parent = parent.parent;
                }
                for (int j = parents.Count - 1; j >= 0; j--)
                {
                    sb.Append(parents[j].gameObject.name);
                    sb.Append("/");
                }
                sb.Append(mono.gameObject.name);
                if (i < monos.Length - 1) sb.Append("\n");
            }
            text = sb.ToString();
            return false;
        }
        else
        {
            text = string.Empty;
            return true;
        }
    }
}