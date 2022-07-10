using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ZetanStudio.BehaviourTree.Nodes;
using Node = ZetanStudio.BehaviourTree.Nodes.Node;
using Action = ZetanStudio.BehaviourTree.Nodes.Action;

namespace ZetanStudio.BehaviourTree.Editor
{
    public class BehaviourTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { }

        public Action<NodeEditor> nodeSelectedCallback;
        public Action<NodeEditor> nodeUnselectedCallback;
        public System.Action undoRecordsChangedCallback;
        public BehaviourTree tree;
        private readonly string nodeUIFile;

        private BehaviourTree treeBef;
        private bool isLocal;
        private readonly BehaviourTreeEditorSettings settings;
        private readonly RuntimeUndo runtimeUndo = new RuntimeUndo();

        public bool CanUndo => runtimeUndo.CanUndo;
        public string UndoName => Tr(runtimeUndo.TopUndoName);
        public bool CanRedo => runtimeUndo.CanRedo;
        public string RedoName => Tr(runtimeUndo.TopRedoName);

        protected override bool canCopySelection => false;
        protected override bool canDuplicateSelection => false;
        protected override bool canPaste => false;
        protected override bool canCutSelection => false;
        protected override bool canDeleteSelection
        {
            get
            {
                int index = selection.FindIndex(x => x is NodeEditor editor && editor.node is Entry);
                if (index > 0) RemoveFromSelection(selection[index]);
                return base.canDeleteSelection;
            }
        }

        public BehaviourTreeView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            settings = BehaviourTreeEditorSettings.GetOrCreate();
            var styleSheet = settings.treeUss;
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
            runtimeUndo.onRecordsChanged += OnUndoRecordsChanged;
            runtimeUndo.undoRedoPerformed += OnUndoRedo;

            nodeUIFile = AssetDatabase.GetAssetPath(settings.nodeUxml);
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
                void selectType(Type type) => CreateTreeNode(type, nodePosition);

                int sort(Type left, Type right)
                {
                    if (left == right) return 0;
                    if (isAction(left))
                        if (isAction(right)) return sortName(left, right);
                        else return -1;
                    else if (isAction(right)) return 1;
                    else if (isConditional(left))
                        if (isConditional(right)) return sortName(left, right);
                        else return -1;
                    else if (isConditional(right)) return 1;
                    else if (isComposite(left))
                        if (isComposite(right)) return sortName(left, right);
                        else return -1;
                    else if (isComposite(right)) return 1;
                    else if (isDecorator(left))
                        if (isDecorator(right)) return sortName(left, right);
                        else return -1;
                    else return 1;

                    int sortName(Type left, Type right)
                    {
                        return string.Compare(group(left) + left.Name, group(right) + right.Name);
                    }
                }
                (string, System.Action)[] actions = new (string, System.Action)[]
                {
                    ("Action",  () => CreateNewScript(ScriptTemplate.Action)),
                    ("Conditional", () => CreateNewScript(ScriptTemplate.Conditional)),
                    ("Composite", () => CreateNewScript(ScriptTemplate.Composite)),
                    ("Decorator", () => CreateNewScript(ScriptTemplate.Decorator))
                };

                string group(Type type)
                {
                    string group = string.Empty;
                    if (isAction(type)) group = Tr("行为结点(Action)");
                    else if (isConditional(type)) group = Tr("条件结点(Conditional)");
                    else if (isComposite(type)) group = Tr("复合结点(Composite)");
                    else if (isDecorator(type)) group = Tr("修饰结点(Decorator)");
                    string subGroup = type.GetCustomAttribute<GroupAttribute>()?.group ?? string.Empty;
                    group += !string.IsNullOrEmpty(subGroup) ? $"/{subGroup}/" : string.Empty;
                    return group;
                }

                var dropdown = new AdvancedDropdown<Type>(TypeCache.GetTypesDerivedFrom<Node>().Where(x => !x.IsAbstract && x != typeof(Entry)), selectType,
                                                          t => t.Name, group, tooltipGetter: Node.GetNodeDesc, sorter: sort, title: Tr("结点"), addCallbacks: actions);
                dropdown.Show(evt.mousePosition, 250f);

                static bool isAction(Type left) => typeof(Action).IsAssignableFrom(left);
                static bool isConditional(Type type) => typeof(Conditional).IsAssignableFrom(type);
                static bool isComposite(Type type) => typeof(Composite).IsAssignableFrom(type);
                static bool isDecorator(Type type) => typeof(Decorator).IsAssignableFrom(type);
            }
            else if (evt.target is NodeEditor editor)
            {
                if (editor.output != null && editor.output.connected && selection.Count < 2)
                    evt.menu.AppendAction(Tr("选择子结点"), (a) => SelectNodeChildren(editor));
                if (editor.node is not Entry)
                {
                    evt.menu.AppendAction(Tr("删除"), (a) => RightClickDeletion());
                    evt.menu.AppendAction(Tr("复制"), (a) => CopyNode(editor));
                }
                if (!editor.node.GetType().IsSealed && selection.Count < 2)
                {
                    evt.menu.AppendSeparator();
                    var scripts = AssetDatabase.FindAssets($"t:monoscript {editor.node.GetType().Name}");
                    evt.menu.AppendAction(Tr("编辑脚本"), (a) =>
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
                    evt.menu.AppendAction(Tr("删除"), (a) => RightClickDeletion());
                }
            }
        }

        public void InsertNode(Type type)
        {
            if (!tree) return;
            var newNode = CreateTreeNode(type, this.ChangeCoordinatesTo(contentViewContainer, viewport.localBound.center - new Vector2(20, 80)));
            if (newNode != null)
            {
                ClearSelection();
                AddToSelection(newNode);
            }
        }
        #endregion

        #region 事件回调
        public void UndoOperation()
        {
            runtimeUndo.PerformUndo();
        }
        public void RedoOperation()
        {
            runtimeUndo.PerformRedo();
        }

        private void OnUndoRecordsChanged()
        {
            undoRecordsChangedCallback?.Invoke();
        }
        private void OnUndoRedo()
        {
            if (tree) DrawTreeView(tree);
        }
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                HashSet<NodeEditor> removedNodes = new HashSet<NodeEditor>();
                //为了保证撤销重做的顺利进行，先遍历结点，再遍历连线
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is NodeEditor editor)
                    {
                        if (isLocal) Undo.RegisterCompleteObjectUndo(tree, Tr("删除结点"));
                        else runtimeUndo.RecordTreeChange(tree, Tr("删除结点"));
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
                        if (!removedNodes.Contains(parent) && !removedNodes.Contains(child))//不是因删除结点引起的断连才记录
                            if (isLocal) Undo.RegisterCompleteObjectUndo(tree, Tr("断开子结点"));
                            else runtimeUndo.RecordTreeChange(tree, Tr("断开子结点"));
                        parent.node.RemoveChild(child.node);
                        UpdateValid(parent);
                        UpdateValid(child);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    if (isLocal) Undo.RegisterCompleteObjectUndo(tree, Tr("连接子结点"));
                    else runtimeUndo.RecordTreeChange(tree, Tr("连接子结点"));
                    NodeEditor parent = edge.output.node as NodeEditor;
                    NodeEditor child = edge.input.node as NodeEditor;
                    parent.node.AddChild(child.node);
                    UpdateValid(parent);
                    UpdateValid(child);
                });
            }

            if (graphViewChange.elementsToRemove != null || graphViewChange.edgesToCreate != null)
                EditorUtility.SetDirty(tree);

            nodes.ForEach((n) =>
            {
                NodeEditor editor = n as NodeEditor;
                editor.TrySort();
            });

            tree.SortPriority();

            return graphViewChange;
        }

        private void UpdateValid(NodeEditor editor)
        {
            if (editor.node is ParentNode) editor.UpdateValid(tree);
        }

        private void OnNodePositionChanged(NodeEditor editor, Vector2 oldPos)
        {
            if (isLocal) Undo.RegisterCompleteObjectUndo(tree, Tr("移动结点"));
            else if (selection.Count > 1) runtimeUndo.RecordMultNodePosition(editor.node, oldPos, Tr("移动结点"));
            else runtimeUndo.RecordNodePosition(editor.node, oldPos, Tr("移动结点"));
        }
        public void OnUpdate()
        {
            nodes.ForEach(n =>
            {
                NodeEditor editor = n as NodeEditor;
                if (Application.isPlaying) editor.UpdateStates();
                editor.UpdateDesc();
                editor.UpdateInvalid(tree);
                editor.UpdateAbortType();
            });
        }
        #endregion

        #region 树相关
        public void Vocate()
        {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
        }
        public void DrawTreeView(BehaviourTree newTree)
        {
            if (newTree)
            {
                if (treeBef != newTree)
                    if (ZetanUtility.Editor.IsLocalAssets(treeBef)) Undo.ClearUndo(treeBef);
                    else runtimeUndo.Clear();
                tree = newTree;
            }
            if (tree != null) treeBef = tree;
            Vocate();
            if (!tree) return;
            isLocal = ZetanUtility.Editor.IsLocalAssets(tree);
            if (string.IsNullOrEmpty(tree.Entry.guid)) tree.Entry.guid = GUID.Generate().ToString();
            tree.Nodes.ForEach(n => CreateNode(n));
            tree.Nodes.ForEach(n => CreateEdges(n));
            tree.Nodes.ForEach(n => { if (n is Composite composite) composite.SortByPosition(); });
        }
        private NodeEditor CreateTreeNode(Type type, Vector2 position)
        {
            if (type.IsSubclassOf(typeof(Node)))
                if (tree.ScenceOnly) return CreateNewNode(Node.GetRuntimeNode(type), $"{ObjectNames.GetUniqueName(tree.Nodes.Select(x => x.name).ToArray(), type.Name)}(R)", position);
                else return CreateNewNode(Activator.CreateInstance(type) as Node, $"{ObjectNames.GetUniqueName(tree.Nodes.Select(x => x.name).ToArray(), type.Name)}", position);
            else return null;
        }
        #endregion

        #region 图形结点相关
        private NodeEditor CreateNewNode(Node newNode, string name, Vector2 position)
        {
            newNode.name = name;
            newNode._position = position;
            newNode.guid = GUID.Generate().ToString();
            if (tree.IsInstance)
            {
                if (!tree.ScenceOnly) newNode.Instantiate();
                tree.InitNode(newNode);
            }
            if (isLocal) Undo.RegisterCompleteObjectUndo(tree, Tr("新增结点"));
            else runtimeUndo.RecordTreeChange(tree, Tr("新增结点"));
            tree.AddNode(newNode);
            return CreateNode(newNode);
        }
        private NodeEditor CreateNode(Node node)
        {
            NodeEditor editor = new NodeEditor(node, nodeSelectedCallback, nodeUnselectedCallback, OnNodePositionChanged, nodeUIFile, settings);
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
            if (isLocal) Undo.RegisterCompleteObjectUndo(tree, Tr("复制结点"));
            else runtimeUndo.RecordTreeChange(tree, Tr("复制结点"));
            BehaviourTree.Traverse(node, n =>
            {
                CreateNewNode(n, $"({tree.Nodes.Count}) {n.GetType().Name}{(tree.ScenceOnly ? "(R)" : string.Empty)}", n._position + new Vector2(30, 30));
            });
            DrawTreeView(tree);
        }
        #endregion

        #region 连线相关
        private void CreateEdges(Node node)
        {
            if (node is ParentNode parentNode)
            {
                NodeEditor parent = GetNodeByGuid(node.guid) as NodeEditor;
                parentNode.GetChildren().ForEach(c =>
                {
                    NodeEditor child = GetNodeByGuid(c.guid) as NodeEditor;
                    Edge edge = parent.output.ConnectTo(child.input);
                    UpdateValid(parent);
                    UpdateValid(child);
                    AddElement(edge);
                });
            }
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node && !BehaviourTree.Reachable((endPort.node as NodeEditor).node, (startPort.node as NodeEditor).node)).ToList();
        }
        #endregion

        private void CreateNewScript(ScriptTemplate template)
        {
            string path = $"{settings.newNodeScriptFolder}/{template.folder}";
            if (path.EndsWith("/")) path = path[..^1];

            UnityEngine.Object script = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            Selection.activeObject = script;
            EditorGUIUtility.PingObject(script);

            string templatePath = AssetDatabase.GetAssetPath(template.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.fileName);
        }

        public string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }

        private class RuntimeUndo
        {
            public System.Action undoRedoPerformed;
            public System.Action onRecordsChanged;

            private readonly Stack<Record> undoRecords = new Stack<Record>();
            private readonly Stack<Record> redoRecords = new Stack<Record>();

            public bool CanUndo => undoRecords.Count > 0;
            public bool CanRedo => redoRecords.Count > 0;

            public string TopUndoName
            {
                get
                {
                    if (undoRecords.Count > 0) return undoRecords.Peek().name;
                    else return "无撤销记录";
                }
            }
            public string TopRedoName
            {
                get
                {
                    if (redoRecords.Count > 0) return redoRecords.Peek().name;
                    else return "无重做记录";
                }
            }

            public void RecordNodePosition(Node node, Vector2 position, string name)
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
                undoRecords.Push(new Record(RecordType.NodePosition, name) { node = node, position = position });
                onRecordsChanged?.Invoke();
            }

            public void RecordMultNodePosition(Node node, Vector2 position, string name)
            {
                //栈顶是多选移动记录，则直接修改此记录
                if (undoRecords.Count > 0 && undoRecords.Peek().type == RecordType.MultNodePosition)
                {
                    if (undoRecords.Peek().positions.ContainsKey(node)) return; //已经记录过该结点了，则不再记录
                    else undoRecords.Peek().positions.Add(node, position);
                }
                //否则新建记录
                else undoRecords.Push(new Record(RecordType.MultNodePosition, name) { positions = new Dictionary<Node, Vector2> { { node, position } } });
                onRecordsChanged?.Invoke();
            }

            public void RecordTreeChange(BehaviourTree tree, string name)
            {
                Dictionary<ParentNode, List<Node>> nodeChildren = new Dictionary<ParentNode, List<Node>>();
                foreach (var node in tree.Nodes)
                {
                    if (node is ParentNode parent)
                        nodeChildren.Add(parent, new List<Node>(parent.GetChildren()));
                }
                undoRecords.Push(new Record(RecordType.TreeChange, name) { tree = tree, nodes = new List<Node>(tree.Nodes), nodeChildren = nodeChildren });
                undoRecords.Peek().evaluatedConditionals.UnionWith(tree.GetEvaluatedComposites());
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
                public string name;
                public RecordType type;
                public BehaviourTree tree;
                public Node node;
                public List<Node> nodes = new List<Node>();
                public Vector2 position;
                public Dictionary<Node, Vector2> positions = new Dictionary<Node, Vector2>();
                public Dictionary<Node, NodeData> nodeDatas = new Dictionary<Node, NodeData>();
                public Dictionary<ParentNode, List<Node>> nodeChildren = new Dictionary<ParentNode, List<Node>>();
                public HashSet<Composite> evaluatedConditionals = new HashSet<Composite>();

                public Record(RecordType type, string name)
                {
                    this.type = type;
                    this.name = name;
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
                        Dictionary<ParentNode, List<Node>> nodeChildren = new Dictionary<ParentNode, List<Node>>();
                        foreach (var node in tree.Nodes)
                        {
                            if (node is ParentNode parent)
                                nodeChildren.Add(parent, new List<Node>(parent.GetChildren()));
                        }
                        Record record = new Record(RecordType.TreeChange, name) { tree = tree, nodes = new List<Node>(tree.Nodes), nodeChildren = nodeChildren };
                        var toDelete = new List<Node>(tree.Nodes.Except(nodes));
                        var toAdd = new List<Node>(nodes.Except(tree.Nodes));
                        foreach (var delete in toDelete)
                        {
                            if (delete is not Entry)
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
                        tree.GetEvaluatedComposites().Clear();
                        tree.GetEvaluatedComposites().UnionWith(evaluatedConditionals);
                        return record;
                    }
                    Record UndoNodePosition()
                    {
                        Vector2 current = node._position;
                        node._position = position;
                        return new Record(RecordType.NodePosition, name) { node = node, position = current };
                    }

                    Record UndoMultNodePosition()
                    {
                        Dictionary<Node, Vector2> currents = new Dictionary<Node, Vector2>();
                        foreach (var kvp in positions)
                        {
                            currents.Add(kvp.Key, kvp.Key._position);
                            kvp.Key._position = kvp.Value;
                        }
                        return new Record(RecordType.MultNodePosition, name) { positions = currents };
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