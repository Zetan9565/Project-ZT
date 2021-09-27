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
        public System.Action undoChangedCallback;
        public BehaviourTree tree;

        private BehaviourTree treeBef;
        private readonly UndoRedo Undo;
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

            Undo = new UndoRedo();
            Undo.undoRedoPerformed += OnUndoRedo;
            Undo.onRecordsChanged += OnUndoChanged;
            UnityEditor.Undo.undoRedoPerformed += OnUndoRedo;
        }

        #region 撤销重做相关
        public void UndoOperation()
        {
            Undo.PerformUndo();
        }
        public void RedoOperation()
        {
            Undo.PerformRedo();
        }

        public bool CanUndo()
        {
            return Undo.CanUndo;
        }
        public bool CanRedo()
        {
            return Undo.CanRedo;
        }
        #endregion

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
                var types = TypeCache.GetTypesDerivedFrom<Action>();
                foreach (var type in types)
                {
                    if (!type.IsAbstract && !type.IsGenericType)
                        evt.menu.AppendAction($"行为结点(Action)/{type.Name}", (a) => CreateTreeNode(type, nodePosition));
                }

                types = TypeCache.GetTypesDerivedFrom<Conditional>();
                foreach (var type in types)
                {
                    if (!type.IsAbstract && !type.IsGenericType)
                        evt.menu.AppendAction($"条件结点(Conditional)/{type.Name}", (a) => CreateTreeNode(type, nodePosition));
                }

                types = TypeCache.GetTypesDerivedFrom<Composite>();
                foreach (var type in types)
                {
                    if (!type.IsAbstract && !type.IsGenericType)
                        evt.menu.AppendAction($"复合结点(Composite)/{type.Name}", (a) => CreateTreeNode(type, nodePosition));
                }

                types = TypeCache.GetTypesDerivedFrom<Decorator>();
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
                if (editor.output != null && editor.output.connected)
                    evt.menu.AppendAction("选择子结点", (a) => SelectNodeChildren(editor));
                if (!(editor.node is Entry))
                {
                    evt.menu.AppendAction("删除", (a) => RightClickDeletion());
                    evt.menu.AppendAction("复制", (a) => CopyNode(editor));
                }
                if (!(editor.node is Entry))
                {
                    evt.menu.AppendSeparator();
                    evt.menu.AppendAction("编辑脚本", (a) => AssetDatabase.OpenAsset(MonoScript.FromScriptableObject(editor.node)));
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
        #endregion

        #region 事件回调
        private void OnUndoChanged()
        {
            undoChangedCallback?.Invoke();
        }
        private void OnUndoRedo()
        {
            DrawTreeView(tree);
            AssetDatabase.SaveAssets();
        }
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null || graphViewChange.edgesToCreate != null)
                Undo.RecordTreeChange(tree);
            if (graphViewChange.elementsToRemove != null)
            {
                //为了保证撤销重做的顺利进行，先遍历结点，再遍历连线
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is NodeEditor editor)
                    {
                        tree.DeleteNode(editor.node);
                        EditorUtility.SetDirty(tree);
                    }

                    if (elem is Edge edge)
                    {
                        NodeEditor parent = edge.output.node as NodeEditor;
                        NodeEditor child = edge.input.node as NodeEditor;
                        parent.node.RemoveChild(child.node);
                        UpdateValid(parent);
                        UpdateValid(child);
                        EditorUtility.SetDirty(parent.node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    NodeEditor parent = edge.output.node as NodeEditor;
                    NodeEditor child = edge.input.node as NodeEditor;
                    parent.node.AddChild(child.node);
                    UpdateValid(parent);
                    UpdateValid(child);
                    EditorUtility.SetDirty(parent.node);
                });
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
            if (!(editor.node is Action) && !(editor.node is Conditional))
                editor.UpdateValid(tree);
        }

        private void OnNodePositionChanged(NodeEditor editor, Vector2 oldPos)
        {
            if (selection.Where(x => x is NodeEditor).Count() > 0) Undo.RecordMultNodePosition(editor.node, oldPos);
            else Undo.RecordNodePosition(editor.node, oldPos);
        }
        public void OnUpdate()
        {
            nodes.ForEach(n =>
            {
                NodeEditor editor = n as NodeEditor;
                if (Application.isPlaying)
                    editor.UpdateStates();
                editor.UpdateInvalid();
                editor.UpdateAbortType();
            });
        }
        #endregion

        #region 树相关
        public void DrawTreeView(BehaviourTree newTree)
        {
            if (newTree)
            {
                if (treeBef != newTree) Undo.Clear();
                tree = newTree;
            }
            if (tree != null) treeBef = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (!tree) return;
            if (EditorUtility.IsPersistent(tree) && tree.Entry == null)
            {
                CreateTreeNode(typeof(Entry), Vector2.zero);
                AssetDatabase.SaveAssets();
            }

            tree.Nodes.ForEach(n => CreateNode(n));
            tree.Nodes.ForEach(n => CreateEdges(n));
            tree.Nodes.ForEach(n => { if (n is Composite composite) composite.SortByPosition(); });
        }
        private void CreateTreeNode(Type type, Vector2 position)
        {
            if (type.IsSubclassOf(typeof(Node)))
                if (tree.IsRuntime) CreateNewNode(Node.GetRuntimeNode(type), $"({tree.Nodes.Count}) {type.Name}(R)", position);
                else CreateNewNode(ScriptableObject.CreateInstance(type) as Node, $"({tree.Nodes.Count}) {type.Name}", position);
        }
        #endregion

        #region 图形结点相关
        private void CreateNewNode(Node newNode, string name, Vector2 position, bool record = true)
        {
            newNode.name = name;
            newNode._position = position;
            newNode.guid = GUID.Generate().ToString();
            if (tree.IsInstance)
            {
                if (!tree.IsRuntime) newNode = newNode.GetInstance();
                newNode.Init(tree);
            }
            if (record) Undo.RecordTreeChange(tree);
            tree.AddNode(newNode);
            CreateNode(newNode);
        }
        private void CreateNode(Node node)
        {
            NodeEditor editor = new NodeEditor(node, nodeSelectedCallback, nodeUnselectedCallback, OnNodePositionChanged);
            AddElement(editor);
        }
        private void RightClickDeletion()
        {
            DeleteSelection();
            DrawTreeView(tree);
        }
        private void CopyNode(NodeEditor editor)
        {
            Node node = editor.node.Copy();
            Undo.RecordTreeChange(tree);
            BehaviourTree.Traverse(node, n =>
            {
                CreateNewNode(n, $"({tree.Nodes.Count}) {n.GetType().Name}{(tree.IsRuntime ? "(R)" : string.Empty)}", n._position + new Vector2(30, 30), false);
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

        private class UndoRedo
        {
            public System.Action undoRedoPerformed;
            public System.Action onRecordsChanged;

            private readonly Stack<Record> undoRecords = new Stack<Record>();
            private readonly Stack<Record> redoRecords = new Stack<Record>();

            public bool CanUndo => undoRecords.Count > 0;
            public bool CanRedo => redoRecords.Count > 0;

            public void RecordNodePosition(Node node, Vector2 position)
            {
                Stack<Record> temp = new Stack<Record>();
                //检查是否记录过该结点
                while (undoRecords.Count > 0 && undoRecords.Peek().type == RecordType.NodePosition)
                {
                    temp.Push(undoRecords.Pop());
                    if (temp.Peek().node == node) //已经记录过该结点了，则不再记录
                    {
                        while (temp.Count > 0)
                        {
                            undoRecords.Push(temp.Pop());
                        }
                        return;
                    }
                }
                while (temp.Count > 0)
                {
                    undoRecords.Push(temp.Pop());
                }
                undoRecords.Push(new Record(RecordType.NodePosition) { node = node, position = position });
                onRecordsChanged?.Invoke();
            }

            public void RecordMultNodePosition(Node node, Vector2 position)
            {
                //栈顶是多选移动记录，则直接修改此记录
                if (undoRecords.Count > 0 && undoRecords.Peek().type == RecordType.MultNodePosition)
                {
                    if (undoRecords.Peek().positions.ContainsKey(node)) return; //已经记录过该结点了，则不再记录
                    else undoRecords.Peek().positions.Add(node, position);
                }
                //否则新建记录
                else undoRecords.Push(new Record(RecordType.MultNodePosition) { positions = new Dictionary<Node, Vector2> { { node, position } } });
                onRecordsChanged?.Invoke();
            }

            public void RecordTreeChange(BehaviourTree tree)
            {
                Dictionary<Node, List<Node>> nodeChildren = new Dictionary<Node, List<Node>>();
                foreach (var node in tree.Nodes)
                {
                    nodeChildren.Add(node, new List<Node>(node.GetChildren()));
                }
                undoRecords.Push(new Record(RecordType.TreeChange) { tree = tree, nodes = new List<Node>(tree.Nodes), nodeChildren = nodeChildren });
                onRecordsChanged?.Invoke();
            }

            public void PerformUndo()
            {
                if (CanUndo)
                {
                    redoRecords.Push(undoRecords.Pop().Perform());
                    undoRedoPerformed?.Invoke();
                    onRecordsChanged?.Invoke();
                }
            }

            public void PerformRedo()
            {
                if (CanRedo)
                {
                    undoRecords.Push(redoRecords.Pop().Perform());
                    undoRedoPerformed?.Invoke();
                    onRecordsChanged?.Invoke();
                }
            }

            public void Clear()
            {
                undoRecords.Clear();
                redoRecords.Clear();
                onRecordsChanged?.Invoke();
            }

            private class Record
            {
                public RecordType type;
                public BehaviourTree tree;
                public Node node;
                public List<Node> nodes = new List<Node>();
                public Vector2 position;
                public Dictionary<Node, Vector2> positions = new Dictionary<Node, Vector2>();
                public Dictionary<Node, NodeData> nodeDatas = new Dictionary<Node, NodeData>();
                public Dictionary<Node, List<Node>> nodeChildren = new Dictionary<Node, List<Node>>();

                public Record(RecordType type)
                {
                    this.type = type;
                }

                public Record Perform()
                {
                    return type switch
                    {
                        RecordType.NodePosition => UndoNodePosition(),
                        RecordType.MultNodePosition => UndoMultNodePosition(),
                        RecordType.TreeChange => UndoTreeChange(),
                        _ => null,
                    };

                    Record UndoTreeChange()
                    {
                        Dictionary<Node, List<Node>> nodeChildren = new Dictionary<Node, List<Node>>();
                        foreach (var node in tree.Nodes)
                        {
                            nodeChildren.Add(node, new List<Node>(node.GetChildren()));
                        }
                        Record record = new Record(RecordType.TreeChange) { tree = tree, nodes = new List<Node>(tree.Nodes), nodeChildren = nodeChildren };
                        var toDelete = new List<Node>(tree.Nodes.Except(nodes));
                        var toAdd = new List<Node>(nodes.Except(tree.Nodes));
                        foreach (var delete in toDelete)
                        {
                            if (!(delete is Entry))
                                tree.DeleteNode(delete);
                        }
                        foreach (var add in toAdd)
                        {
                            tree.AddNode(add);
                        }
                        foreach (var kvp in this.nodeChildren)
                        {
                            toDelete = new List<Node>(kvp.Key.GetChildren().Except(kvp.Value));
                            toAdd = new List<Node>(kvp.Value.Except(kvp.Key.GetChildren()));
                            foreach (var delete in toDelete)
                            {
                                kvp.Key.RemoveChild(delete);
                            }
                            foreach (var add in toAdd)
                            {
                                kvp.Key.AddChild(add);
                            }
                        }
                        return record;
                    }
                    Record UndoNodePosition()
                    {
                        Vector2 current = node._position;
                        node._position = position;
                        return new Record(RecordType.NodePosition) { node = node, position = current };
                    }

                    Record UndoMultNodePosition()
                    {
                        Dictionary<Node, Vector2> currents = new Dictionary<Node, Vector2>();
                        foreach (var kvp in positions)
                        {
                            currents.Add(kvp.Key, kvp.Key._position);
                            kvp.Key._position = kvp.Value;
                        }
                        return new Record(RecordType.MultNodePosition) { positions = currents };
                    }
                }

                public class NodeData
                {
                    public Node parent;
                    public List<Node> children;

                    public NodeData(Node parent, List<Node> children)
                    {
                        this.parent = parent;
                        this.children = children;
                    }
                }
            }

            private enum RecordType
            {
                Empty,
                NodePosition,
                MultNodePosition,
                TreeChange,
            }
        }
    }
}