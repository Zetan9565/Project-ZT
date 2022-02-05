using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.BehaviourTree
{
    public partial class BehaviourTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { }

        public Action<NodeEditor> nodeSelectedCallback;
        public Action<NodeEditor> nodeUnselectedCallback;
        public BehaviourTree tree;

        private BehaviourTree treeBef;
        private readonly BehaviourTreeSettings settings;

        public BehaviourTreeView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            settings = BehaviourTreeSettings.GetOrCreate();
            var styleSheet = settings.treeUss;
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        #region 操作相关
        private void SelectNodeChildren(NodeEditor editor)
        {
            if (editor != null && editor.output != null)
                foreach (var edge in editor.output.connections)
                {
                    AddToSelection(edge.input.node);
                    SelectNodeChildren(edge.input.node as NodeEditor);
                }
        }
        public override EventPropagation DeleteSelection()
        {
            for (int i = 0; i < selection.Count; i++)
            {
                if (selection[i] is NodeEditor editor && editor.node is Entry)
                {
                    RemoveFromSelection(selection[i]);
                    i--;
                }
            }
            return base.DeleteSelection();
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!tree) return;

            if (evt.target == this)
            {
                Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
                var types = TypeCache.GetTypesDerivedFrom<Action>().OrderBy(x => x.Name);
                foreach (var type in types)
                {
                    if (!type.IsAbstract && !type.IsGenericType)
                        evt.menu.AppendAction($"行为结点(Action)/{type.Name}", (a) => CreateTreeNode(type, nodePosition));
                }

                types = TypeCache.GetTypesDerivedFrom<Conditional>().OrderBy(x => x.Name);
                foreach (var type in types)
                {
                    if (!type.IsAbstract && !type.IsGenericType)
                        evt.menu.AppendAction($"条件结点(Conditional)/{type.Name}", (a) => CreateTreeNode(type, nodePosition));
                }

                types = TypeCache.GetTypesDerivedFrom<Composite>().OrderBy(x => x.Name);
                foreach (var type in types)
                {
                    if (!type.IsAbstract && !type.IsGenericType)
                        evt.menu.AppendAction($"复合结点(Composite)/{type.Name}", (a) => CreateTreeNode(type, nodePosition));
                }

                types = TypeCache.GetTypesDerivedFrom<Decorator>().OrderBy(x => x.Name);
                foreach (var type in types)
                {
                    if (!type.IsAbstract && !type.IsGenericType)
                        evt.menu.AppendAction($"修饰结点(Decorator)/{type.Name}", (a) => CreateTreeNode(type, nodePosition));
                }

                evt.menu.AppendSeparator();
                evt.menu.AppendAction("新建/Action", (a) => CreateNewScript(ScriptTemplate.Action));
                evt.menu.AppendAction("新建/Conditional", (a) => CreateNewScript(ScriptTemplate.Conditional));
                evt.menu.AppendAction("新建/Composite", (a) => CreateNewScript(ScriptTemplate.Composite));
                evt.menu.AppendAction("新建/Decorator", (a) => CreateNewScript(ScriptTemplate.Decorator));
            }
            else if (evt.target is NodeEditor editor)
            {
                if (editor.output != null && editor.output.connected && selection.Count < 2)
                    evt.menu.AppendAction("选择子结点", (a) => SelectNodeChildren(editor));
                if (editor.node is not Entry)
                {
                    evt.menu.AppendAction("删除", (a) => RightClickDeletion());
                    evt.menu.AppendAction("复制", (a) => CopyNode(editor));
                }
                if (!editor.node.GetType().IsSealed && selection.Count < 2)
                {
                    evt.menu.AppendSeparator();
                    var scripts = AssetDatabase.FindAssets($"t:monoscript {editor.node.GetType().Name}");
                    evt.menu.AppendAction("编辑脚本", (a) =>
                    {
                        foreach (var s in scripts)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(s);
                            if (path.Contains($"/{editor.node.GetType().Name}.cs"))
                                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(path));
                        }
                    });
                }
            }
            else if (evt.target is Edge edge)
            {
                if (edge.output.node is NodeEditor || edge.input.node is NodeEditor)
                {
                    evt.menu.AppendAction("删除", (a) => RightClickDeletion());
                }
            }
        }
        public void InsertNode(Type type)
        {
            var newNode = CreateTreeNode(type, this.ChangeCoordinatesTo(contentViewContainer, viewport.localBound.center - new Vector2(20, 80)));
            if (newNode != null)
            {
                ClearSelection();
                AddToSelection(newNode);
            }
        }
        #endregion

        #region 事件回调
        private void OnUndoRedo()
        {
            if (tree)
            {
                DrawTreeView(tree);
                AssetDatabase.SaveAssetIfDirty(tree);
            }
        }
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            //if (graphViewChange.elementsToRemove != null && graphViewChange.elementsToRemove.Exists(elem => elem is NodeEditor))
            //    Undo.RegisterCompleteObjectUndo(tree, "行为树变化");
            if (graphViewChange.elementsToRemove != null)
            {
                HashSet<NodeEditor> removedNodes = new HashSet<NodeEditor>();
                //为了保证撤销重做的顺利进行，先遍历结点，再遍历连线
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is NodeEditor editor)
                    {
                        tree.DeleteNode(editor.node);
                        removedNodes.Add(editor);
                    }
                });
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is Edge edge)
                    {
                        NodeEditor parent = edge.output.node as NodeEditor;
                        NodeEditor child = edge.input.node as NodeEditor;
                        if (!removedNodes.Contains(parent) && !removedNodes.Contains(child))
                            Undo.RegisterCompleteObjectUndo(tree, "断开子结点");
                        parent.node.RemoveChild(child.node);
                        UpdateValid(parent);
                        UpdateValid(child);
                    }
                });
                EditorUtility.SetDirty(tree);
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    Undo.RegisterCompleteObjectUndo(tree, "链接子结点");
                    NodeEditor parent = edge.output.node as NodeEditor;
                    NodeEditor child = edge.input.node as NodeEditor;
                    parent.node.AddChild(child.node);
                    UpdateValid(parent);
                    UpdateValid(child);
                });
                EditorUtility.SetDirty(tree);
            }

            nodes.ForEach((n) =>
            {
                NodeEditor editor = n as NodeEditor;
                editor.TrySort();
            });

            return graphViewChange;
        }

        private void UpdateValid(NodeEditor editor)
        {
            if (editor.node is not Action && editor.node is not Conditional)
                editor.UpdateValid(tree);
        }

        private void OnNodePositionChanged(NodeEditor editor, Vector2 oldPos)
        {
            Undo.RegisterCompleteObjectUndo(tree, "移动结点");
        }
        public void OnUpdate()
        {
            nodes.ForEach(n =>
            {
                NodeEditor editor = n as NodeEditor;
                if (Application.isPlaying)
                    editor.UpdateStates();
                editor.UpdateDesc();
                editor.UpdateInvalid(tree);
                editor.UpdateAbortType();
            });
        }
        #endregion

        #region 树相关
        public void DrawTreeView(BehaviourTree newTree)
        {
            if (newTree)
            {
                if (treeBef != newTree) Undo.ClearUndo(treeBef);
                tree = newTree;
            }
            if (tree != null) treeBef = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (!tree) return;
            if (string.IsNullOrEmpty(tree.Entry.guid))
                tree.Entry.guid = UnityEditor.GUID.Generate().ToString();
            tree.Nodes.ForEach(n => CreateNode(n));
            tree.Nodes.ForEach(n => CreateEdges(n));
            tree.Nodes.ForEach(n => { if (n is Composite composite) composite.SortByPosition(); });
        }
        private NodeEditor CreateTreeNode(Type type, Vector2 position)
        {
            if (type.IsSubclassOf(typeof(Node)))
                if (tree.IsRuntime) return CreateNewNode(Node.GetRuntimeNode(type), $"({tree.Nodes.Count}) {type.Name}(R)", position);
                else return CreateNewNode(Activator.CreateInstance(type) as Node, $"({tree.Nodes.Count}) {type.Name}", position);
            else return null;
        }
        #endregion

        #region 图形结点相关
        private NodeEditor CreateNewNode(Node newNode, string name, Vector2 position, bool record = true)
        {
            newNode.name = name;
            newNode._position = position;
            newNode.guid = GUID.Generate().ToString();
            if (tree.IsInstance)
            {
                if (!tree.IsRuntime) newNode = newNode.GetInstance();
                newNode.Init(tree);
            }
            if (record) Undo.RegisterCompleteObjectUndo(tree, "新增结点");
            tree.AddNode(newNode);
            return CreateNode(newNode);
        }
        private NodeEditor CreateNode(Node node)
        {
            NodeEditor editor = new NodeEditor(node, nodeSelectedCallback, nodeUnselectedCallback, OnNodePositionChanged);
            UpdateValid(editor);
            AddElement(editor);
            return editor;
        }
        private void RightClickDeletion()
        {
            DeleteSelection();
            DrawTreeView(tree);
        }
        private void CopyNode(NodeEditor editor)
        {
            Node node = editor.node.Copy();
            Undo.RegisterCompleteObjectUndo(tree, "复制结点");
            BehaviourTree.Traverse(node, n =>
            {
                CreateNewNode(n, $"({tree.Nodes.Count}) {n.GetType().Name}{(tree.IsRuntime ? "(R)" : string.Empty)}", n._position + new Vector2(30, 30));
            });
            DrawTreeView(tree);
        }
        #endregion

        #region 连线相关
        private void CreateEdges(Node node)
        {
            node.GetChildren().ForEach(c =>
            {
                NodeEditor parent = GetNodeByGuid(node.guid) as NodeEditor;
                NodeEditor child = GetNodeByGuid(c.guid) as NodeEditor;
                Edge edge = parent.output.ConnectTo(child.input);
                UpdateValid(parent);
                UpdateValid(child);
                AddElement(edge);
            });
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }
        #endregion

        private void CreateNewScript(ScriptTemplate template)
        {
            string path = $"{settings.newNodeScriptFolder}/{template.folder}";
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);

            UnityEngine.Object script = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            Selection.activeObject = script;
            EditorGUIUtility.PingObject(script);

            string templatePath = AssetDatabase.GetAssetPath(template.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.fileName);
        }
    }
}