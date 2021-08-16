using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class BehaviourTreeGraphWindow : EditorWindow
{
    private BehaviourTreeGraphView graphView;

    [MenuItem("Zetan Studio/行为树窗口")]
    public static void OpenBehaviourTreeWindow()
    {
        var window = GetWindow<BehaviourTreeGraphWindow>();
        window.titleContent = new GUIContent("编辑行为树");
        window.ConstructGraphView();
        window.CreateToolbar();
    }
    public static void OpenBehaviourTreeWindow(BehaviourTree tree)
    {
        var window = GetWindow<BehaviourTreeGraphWindow>();
        window.titleContent = new GUIContent("编辑行为树");
        window.ConstructGraphView(tree);
        window.CreateToolbar();
    }

    private void ConstructGraphView()
    {
        graphView = new BehaviourTreeGraphView()
        {
            name = "行为树",
        };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void ConstructGraphView(BehaviourTree tree)
    {
        graphView = new BehaviourTreeGraphView(tree)
        {
            name = "行为树",
        };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void CreateToolbar()
    {
        var tb = new Toolbar();
        var creationBtn = new Button(
        delegate
        {
            graphView.AddNode("行为", new Vector2(position.width * 0.5f, position.y * 0.5f));
        });
        creationBtn.text = "新建结点";
        tb.Add(creationBtn);

        rootVisualElement.Add(tb);
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }
}
