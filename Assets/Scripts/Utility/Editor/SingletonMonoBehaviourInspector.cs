using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SingletonMonoBehaviour<>), true)]
public class SingletonMonoBehaviourInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (!CheckValid(out string text))
            EditorGUILayout.HelpBox(text, MessageType.Error);
        else base.OnInspectorGUI();
    }

    protected bool CheckValid(out string text)
    {
        var monos = FindObjectsOfType(target.GetType());
        if (monos.Length > 1)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("场景中存在多个<");
            sb.Append(target.GetType()); sb.Append(">，请移除其中一个！\n");
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