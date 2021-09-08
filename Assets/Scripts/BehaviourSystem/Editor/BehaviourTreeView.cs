using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.BehaviourTree
{
    public class BehaviourTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { }

        public Action<NodeEditor> nodeSelectedCallback;
        public Action<NodeEditor> nodeUnselectedCallback;
        public System.Action undoChangedCallback;
        public BehaviourTree tree;

        private readonly UndoRedo Undo;
        private readonly BehaviourTreeSettings settings;

        public BehaviourTreeView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            //this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            settings = BehaviourTreeSettings.GetOrCreateSettings();
            var styleSheet = settings.treeUss;// AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/BehaviourSystem/Editor/BehaviourTreeEditor.uss");
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
                    evt.menu.AppendAction($"行为结点(Action)/{type.Name}", (a) => CreateTreeNode(type, nodePosition));
                }

                types = TypeCache.GetTypesDerivedFrom<Conditional>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"条件结点(Conditional)/{type.Name}", (a) => CreateTreeNode(type, nodePosition));
                }

                types = TypeCache.GetTypesDerivedFrom<Composite>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"复合结点(Composite)/{type.Name}", (a) => CreateTreeNode(type, nodePosition));
                }

                types = TypeCache.GetTypesDerivedFrom<Decorator>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"修饰结点(Decorator)/{type.Name}", (a) => CreateTreeNode(type, nodePosition));
                }

                evt.menu.AppendSeparator();
                evt.menu.AppendAction("新建/Action", (a) => CreateNewScript(ScriptTemplate.DefaultTemplates[0]));
                evt.menu.AppendAction("新建/Conditional", (a) => CreateNewScript(ScriptTemplate.DefaultTemplates[1]));
                evt.menu.AppendAction("新建/Composite", (a) => CreateNewScript(ScriptTemplate.DefaultTemplates[2]));
                evt.menu.AppendAction("新建/Decorator", (a) => CreateNewScript(ScriptTemplate.DefaultTemplates[3]));
            }
            else if (evt.target is NodeEditor editor)
            {
                if (editor.output != null && editor.output.connected)
                    evt.menu.AppendAction("选择子结点", (a) => SelectNodeChildren(editor));
                if (!(editor.node is Entry))
                {
                    evt.menu.AppendAction("删除", (a) => DeleteCurrent());
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
                    evt.menu.AppendAction("删除", (a) => DeleteCurrent());
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
            if (graphViewChange.elementsToRemove != null)
            {
                bool shoulRecordChild = true;
                //为了保证撤销重做的顺利进行，先遍历结点，再遍历连线
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is NodeEditor editor)
                    {
                        shoulRecordChild = false;
                        DeleteTreeNode(editor.node, graphViewChange.elementsToRemove.Where(x => x is NodeEditor).Count() > 1);
                        EditorUtility.SetDirty(tree);
                    }
                });
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is Edge edge)
                    {
                        NodeEditor parent = edge.output.node as NodeEditor;
                        NodeEditor child = edge.input.node as NodeEditor;
                        if (shoulRecordChild) Undo.RecordRemoveChild(parent.node, child.node);
                        parent.node.RemoveChild(child.node);
                        parent.UpdateValid();
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
                    Undo.RecordAddChild(tree, parent.node, child.node);
                    parent.node.AddChild(child.node);
                    parent.UpdateValid();
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
        private void OnNodePositionChanged(NodeEditor editor, Vector2 oldPos)
        {
            if (selection.Where(x => x is NodeEditor).Count() > 0) Undo.RecordMultNodePosition(editor.node, oldPos);
            else Undo.RecordNodePosition(editor.node, oldPos);
        }
        public void OnUpdate()
        {
            if (Application.isPlaying)
                nodes.ForEach(n =>
                {
                    NodeEditor editor = n as NodeEditor;
                    editor.UpdateStates();
                });
        }
        #endregion

        #region 树相关
        public void DrawTreeView(BehaviourTree tree)
        {
            if (tree) this.tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (!this.tree) return;
            if (EditorUtility.IsPersistent(this.tree) && this.tree.Entry == null)
            {
                CreateTreeNode(typeof(Entry), Vector2.zero);
                AssetDatabase.SaveAssets();
            }

            this.tree.Nodes.ForEach(n => CreateNode(n));
            this.tree.Nodes.ForEach(n => CreateEdges(n));
        }
        private void CreateTreeNode(Type type, Vector2 position)
        {
            if (type.IsSubclassOf(typeof(Node))) CreateNewNode(ScriptableObject.CreateInstance(type) as Node, $"({tree.Nodes.Count}) {type.Name}", position);
        }
        private void DeleteTreeNode(Node node, bool mult)
        {
            if (mult) Undo.RecordMultDeleteNode(tree, node);
            else Undo.RecordDeleteNode(tree, node);
            tree.DeleteNode(node);
        }
        #endregion

        #region 图形结点相关
        private void CreateNewNode(Node newNode, string name, Vector2 position)
        {
            newNode.name = name;
            newNode.position = position;
            newNode.guid = GUID.Generate().ToString();
            if (tree.IsInstance)
            {
                newNode = newNode.GetInstance();
                newNode.Init(tree);
            }
            if (!(newNode is Entry)) Undo.RecordAddNode(tree, newNode);
            tree.AddNode(newNode);
            CreateNode(newNode);
        }
        private void CreateNode(Node node)
        {
            NodeEditor editor = new NodeEditor(node, nodeSelectedCallback, nodeUnselectedCallback, OnNodePositionChanged);
            AddElement(editor);
        }
        private void DeleteCurrent()
        {
            DeleteSelection();
            DrawTreeView(tree);
        }
        private void CopyNode(NodeEditor editor)
        {
            CreateNewNode(UnityEngine.Object.Instantiate(editor.node), editor.node.name, editor.node.position + new Vector2(30, 30));
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
            string path = $"{settings.newScriptFolder}/{template.folder}";
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
            public ScriptableObject empty;

            private Stack<Record> undoRecords = new Stack<Record>();
            private Stack<Record> redoRecords = new Stack<Record>();

            public bool CanUndo => undoRecords.Count > 0;
            public bool CanRedo => redoRecords.Count > 0;

            public UndoRedo()
            {
                empty = ScriptableObject.CreateInstance<ScriptableObject>();
            }

            public void RecordAddNode(BehaviourTree tree, Node node)
            {
                undoRecords.Push(new Record(RecordType.AddNode) { tree = tree, node = node });
                onRecordsChanged?.Invoke();
            }

            public void RecordDeleteNode(BehaviourTree tree, Node node)
            {
                undoRecords.Push(new Record(RecordType.DeleteNode) { tree = tree, node = node, parent = tree.FindParent(node), children = node.GetChildren() });
                onRecordsChanged?.Invoke();
            }

            public void RecordMultDeleteNode(BehaviourTree tree, Node node)
            {
                //栈顶是多选删除记录，则直接修改此记录
                if (undoRecords.Count > 0 && undoRecords.Peek().type == RecordType.MultDeleteNode)
                {
                    if (undoRecords.Peek().nodeDatas.ContainsKey(node)) return; //正常情况下不会同时多次删除同一个结点，但还是做一下防备
                    else undoRecords.Peek().nodeDatas.Add(node, new Record.NodeData(tree.FindParent(node), node.GetChildren()));
                }
                //否则新建记录
                else undoRecords.Push(new Record(RecordType.MultDeleteNode) { tree = tree, nodeDatas = new Dictionary<Node, Record.NodeData>() { { node, new Record.NodeData(tree.FindParent(node), node.GetChildren()) } } });
                onRecordsChanged?.Invoke();
            }

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


            public void RecordAddChild(BehaviourTree tree, Node node, Node child)
            {
                undoRecords.Push(new Record(RecordType.AddChild) { node = node, parent = tree.FindParent(child), children = new List<Node> { child } });
                onRecordsChanged?.Invoke();
            }

            public void RecordRemoveChild(Node node, Node child)
            {
                undoRecords.Push(new Record(RecordType.RemoveChild) { node = node, children = new List<Node> { child } });
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

            private class Record
            {
                public RecordType type;
                public BehaviourTree tree;
                public Node node;
                public List<Node> nodes = new List<Node>();
                public Vector2 position;
                public Dictionary<Node, Vector2> positions = new Dictionary<Node, Vector2>();
                public Node parent;
                public List<Node> children = new List<Node>();
                public Dictionary<Node, NodeData> nodeDatas = new Dictionary<Node, NodeData>();

                public Record(RecordType type)
                {
                    this.type = type;
                }

                public Record Perform()
                {
                    switch (type)
                    {
                        case RecordType.NodePosition:
                            Vector2 current = node.position;
                            node.position = position;
                            return new Record(RecordType.NodePosition) { node = node, position = current };
                        case RecordType.MultNodePosition:
                            Dictionary<Node, Vector2> currents = new Dictionary<Node, Vector2>();
                            foreach (var kvp in positions)
                            {
                                currents.Add(kvp.Key, kvp.Key.position);
                                kvp.Key.position = kvp.Value;
                            }
                            return new Record(RecordType.MultNodePosition) { positions = currents };
                        case RecordType.AddNode:
                            var p = tree.FindParent(node);
                            tree.DeleteNode(node);
                            return new Record(RecordType.DeleteNode) { tree = tree, node = node, parent = p, children = node.GetChildren() };
                        case RecordType.MultAddNode:
                            Dictionary<Node, NodeData> deletes = new Dictionary<Node, NodeData>();
                            foreach (var node in nodes)
                            {
                                deletes.Add(node, new NodeData(tree.FindParent(node), node.GetChildren()));
                            }
                            foreach (var node in nodes)
                            {
                                tree.DeleteNode(node);
                            }
                            return new Record(RecordType.MultDeleteNode) { tree = tree, nodeDatas = deletes };
                        case RecordType.DeleteNode:
                            tree.AddNode(node);
                            if (parent) parent.AddChild(node);
                            foreach (var child in children)
                            {
                                node.AddChild(child);
                            }
                            return new Record(RecordType.AddNode) { tree = tree, node = node };
                        case RecordType.MultDeleteNode:
                            List<Node> adds = new List<Node>();
                            foreach (var kvp in nodeDatas)
                            {
                                adds.Add(kvp.Key);
                                tree.AddNode(kvp.Key);
                                if (kvp.Value.parent) kvp.Value.parent.AddChild(kvp.Key);
                                foreach (var child in kvp.Value.children)
                                {
                                    kvp.Key.AddChild(child);
                                }
                            }
                            return new Record(RecordType.MultAddNode) { tree = tree, nodes = adds };
                        case RecordType.AddChild:
                            node.RemoveChild(children[0]);
                            if (parent) parent.AddChild(children[0]);
                            return new Record(RecordType.RemoveChild) { node = node, parent = parent, children = new List<Node>() { children[0] } };
                        case RecordType.RemoveChild:
                            node.AddChild(children[0]);
                            if (parent) parent.RemoveChild(children[0]);
                            return new Record(RecordType.AddChild) { node = node, parent = parent, children = new List<Node>() { children[0] } };
                        case RecordType.Empty:
                        default:
                            return null;
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
                AddNode,
                MultAddNode,
                DeleteNode,
                MultDeleteNode,
                AddChild,
                RemoveChild
            }
        }
        private struct ScriptTemplate
        {
            public string fileName;
            public string folder;
            public TextAsset templateFile;

            public static readonly ScriptTemplate[] DefaultTemplates =
            {
                new ScriptTemplate{ templateFile = BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateAction, fileName = "NewActione.cs", folder = "Action" },
                new ScriptTemplate{ templateFile = BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateComposite, fileName = "NewConditional.cs", folder = "Conditional" },
                new ScriptTemplate{ templateFile = BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateComposite, fileName = "NewComposite.cs", folder = "Composite" },
                new ScriptTemplate{ templateFile = BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateDecorator, fileName = "NewDecorator.cs", folder = "Decorator" },
            };
        }
    }
}