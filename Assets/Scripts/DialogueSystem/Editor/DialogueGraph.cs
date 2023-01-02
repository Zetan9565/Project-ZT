using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.DialogueSystem.Editor
{
    using Extension;

    public class DialogueGraph : GraphView
    {
        public new class UxmlFactory : UxmlFactory<DialogueGraph, UxmlTraits> { }

        #region 声明
        public readonly DialogueEditorSettings settings;
        public Action<DialogueGraphNode> nodeSelectedCallback;
        public Action<DialogueGraphNode> nodeUnselectedCallback;
        private readonly MiniMap miniMap;
        private DialogueGraphNode entry;
        private DialogueGraphNode exit;
        private DialogueGroup invalid;
        private Dialogue dialogue;
        private readonly VisualElement errors;
        public SerializedObject serializedDialog;

        private List<DialogueGroupData> copiedGroups;
        private List<DialogueNode> copiedNodes;
        private GenericData copiedData;
        private Vector2 localMousePosition;
        private Vector2 copiedPosition;
        #endregion

        #region 属性
        public Dialogue Dialogue { get => dialogue; set => ViewDialogue(value); }
        public SerializedProperty SerializedNodes { get; private set; }
        public SerializedProperty SerializedGroups { get; private set; }
        protected override bool canPaste => copiedNodes != null && copiedData != null;
        #endregion

        public DialogueGraph()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer() { minScale = 0.15f });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            settings = DialogueEditorSettings.GetOrCreate();
            styleSheets.Add(settings.editorUss);

            Add(miniMap = new MiniMap());
            miniMap.style.backgroundColor = new StyleColor(new Color32(29, 29, 30, 200));

            var v = new VisualElement();
            v.pickingMode = PickingMode.Ignore;
            v.style.position = Position.Absolute;
            v.style.left = 0;
            v.style.right = 0;
            v.style.top = 0;
            v.style.bottom = 0;
            v.style.flexDirection = FlexDirection.ColumnReverse;
            v.style.alignItems = Align.FlexStart;
            Add(v);
            v.Add(errors = new VisualElement());
            errors.style.flexDirection = FlexDirection.ColumnReverse;
            errors.style.paddingLeft = 3f;
            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                miniMap.style.width = miniMap.maxWidth = evt.newRect.width / 4;
                miniMap.style.height = miniMap.maxHeight = evt.newRect.height / 4;
            });
            Undo.undoRedoPerformed += OnUndoPerformed;
            RegisterCallback<MouseEnterEvent>(evt =>
            {
                localMousePosition = evt.localMousePosition;
            });
            RegisterCallback<MouseMoveEvent>(evt =>
            {
                localMousePosition = evt.localMousePosition;
            });

            serializeGraphElements = OnSerializeGraphElements;
            unserializeAndPaste = OnUnserializeAndPaste;
            elementsAddedToGroup = OnElementsAddedToGroup;
            elementsRemovedFromGroup = OnElementsRemovedFromGroup;
        }

        #region 重写
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!Dialogue) return;
            if (evt.target == this)
            {
                Vector2 position = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
                evt.menu.AppendAction(Tr("分组"), a =>
                {
                    CreateGroup(position);
                });
                if (canPaste) evt.menu.AppendAction(Tr("粘贴"), a => PasteCallback());
                evt.menu.AppendSeparator();
                foreach (var type in TypeCache.GetTypesDerivedFrom<DialogueNode>())
                {
                    if (!type.IsAbstract && type != typeof(EntryNode) && type != typeof(ExitNode))
                        evt.menu.AppendAction($"{GetMenuGroup(type)}{DialogueNode.GetName(type)}", a => CreateNode(type, position));
                }
            }
            else if (evt.target is DialogueGraphNode node)
            {
                if (node.Target is not EntryNode && node != exit)
                {
                    evt.menu.AppendAction(Tr("删除"), a => DeleteSelection());
                    evt.menu.AppendAction(Tr("复制"), a => CopySelectionCallback());
                }
                IEnumerable<Node> dnodes = selection.FindAll(s => s is DialogueGraphNode).Cast<Node>();
                if (node.GetContainingScope() is DialogueGroup group && dnodes.All(s => (s as DialogueGraphNode)!.GetContainingScope() == group))
                {
                    evt.menu.AppendAction(Tr("移出本组"), a =>
                    {
                        var nodes = dnodes.Where(s => s is DialogueGraphNode n && n.GetContainingScope() == group);
                        OnBeforeModify();
                        foreach (var n in nodes)
                        {
                            group.Group.nodes.Remove(n.viewDataKey);
                            n.SetPosition(new Rect(n.layout.position + new Vector2(-10, 10), n.layout.size));
                        }
                        group.RemoveElementsWithoutNotification(nodes);
                    });
                }
                if (dnodes.All(s => (s as DialogueGraphNode)!.GetContainingScope() is null))
                {
                    evt.menu.AppendAction(Tr("添加到") + '/' + Tr("新建分组"), a =>
                    {
                        OnBeforeModify();
                        List<Vector2> positions = new List<Vector2>();
                        foreach (var s in selection)
                        {
                            if (s is Node n)
                                positions.Add(n.layout.center);
                        }
                        var group = CreateGroup(GetCenter(positions));
                        group.AddElements(dnodes);
                    });
                    List<DialogueGroup> groups = new List<DialogueGroup>(graphElements.Where(g => g is DialogueGroup).Cast<DialogueGroup>());
                    if (groups.Count > 0)
                    {
                        OnBeforeModify();
                        evt.menu.AppendSeparator("添加到/");
                        Dictionary<string, int> d = new Dictionary<string, int>();
                        foreach (var g in groups)
                        {
                            var count = 0;
                            if (d.ContainsKey(g.title)) count = d[g.title] + 1;
                            d[g.title] = count;
                            evt.menu.AppendAction(Tr("添加到") + '/' + g.title + (count > 0 ? $" (重名 {count})" : string.Empty), a =>
                            {
                                g.AddElements(dnodes);
                            });
                        }
                    }
                }
                if (node.Target is RecursionSuffix recursion && Dialogue.Reachable(recursion))
                    evt.menu.AppendAction(Tr("选中递归点"), a =>
                    {
                        if (recursion.FindRecursionPoint(Dialogue.Entry) is DialogueNode find)
                        {
                            ClearSelection();
                            AddToSelection(GetNodeByGuid(find.ID));
                            FrameSelection();
                        }
                    });
            }

        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.Where(endPort => canLink(startPort, endPort)).ToList();

            static bool canLink(Port startPort, Port endPort)
            {
                if (endPort.direction == startPort.direction) return false;
                if (endPort.node == startPort.node) return false;
                if (startPort.direction == Direction.Input) (startPort, endPort) = (endPort, startPort);
                if (Dialogue.Reachable(node(endPort), node(startPort))) return false;
                if (node(startPort) is BranchNode && node(endPort) is not ConditionNode) return false;
                return node(endPort).CanLinkFrom(node(startPort), startPort.userData as DialogueOption);

                static DialogueNode node(Port port)
                {
                    return port.node.userData as DialogueNode;
                }
            }
        }
        public override EventPropagation DeleteSelection()
        {
            RemoveFromSelection(entry);
            RemoveFromSelection(exit);
            selection.ForEach(s =>
            {
                if (s is DialogueGroup group)
                {
                    try
                    {
                        group.RemoveElementsWithoutNotification(new GraphElement[] { entry, exit });
                    }
                    catch { }
                }
            });
            return base.DeleteSelection();
        }
        #endregion

        #region 创建相关
        private void CreateNode(Type type, Vector2 position, Action<DialogueGraphNode> callback = null)
        {
            OnBeforeModify();
            var node = Dialogue.Editor.AddNode(Dialogue, type);
            if (node != null)
            {
                node._position = position;
                serializedDialog.Update();
                var gNode = CreateGraphNode(node);
                callback?.Invoke(gNode);
                ClearSelection();
                AddToSelection(gNode);
            }
        }
        private DialogueGroup CreateGroup(Vector2 position)
        {
            var group = new DialogueGroupData(Tr("新分组"), position);
            OnBeforeModify();
            dialogue.groups.Add(group);
            var g = CreateGroup(group);
            g.FocusTitleTextField();
            return g;
        }

        private DialogueGraphNode CreateGraphNode(DialogueNode node)
        {
            var gNode = new DialogueGraphNode(this, node, OnBeforeModify, nodeSelectedCallback, nodeUnselectedCallback, OnDeleteOutput);
            AddElement(gNode);
            if (node is EntryNode) entry = gNode;
            return gNode;
        }
        private void CreateEdges(DialogueNode node)
        {
            if (!node) return;
            DialogueGraphNode parent = GetNodeByGuid(node.ID) as DialogueGraphNode;
            if (node.ExitHere && node is not SuffixNode) AddElement(parent.Outputs[0].ConnectTo(exit.Input));
            else
            {
                if (node is not SuffixNode)
                    for (int i = 0; i < node.Options.Count; i++)
                    {
                        var o = node.Options[i];
                        if (o.Next != null)
                        {
                            DialogueGraphNode child = GetNodeByGuid(o.Next.ID) as DialogueGraphNode;
                            AddElement(parent.Outputs[i].ConnectTo(child?.Input));
                        }
                    }
            }
        }
        private DialogueGroup CreateGroup(DialogueGroupData g)
        {
            var nodes = new HashSet<string>(g.nodes);
            DialogueGroup group = new DialogueGroup(base.nodes.Where(n => nodes.Contains(n.viewDataKey)), g, OnGroupRightClick, OnBeforeModify);
            AddElement(group);
            return group;
        }
        #endregion

        #region 操作回调
        public void OnEdgeDropOutside(DialogueOutput output, Vector2 nodePosition)
        {
            DialogueGraphNode from = output.node;
            if (from.userData is RecursionSuffix || nodes.Any(x => x.ContainsPoint(x.WorldToLocal(nodePosition)))) return;
            var option = output.userData as DialogueOption;
            nodePosition = contentViewContainer.WorldToLocal(nodePosition);
            var exitHere = from.Target.ExitHere;
            GenericMenu menu = new GenericMenu();
            if (from.Outputs.Count == 1 && output.Option.IsMain && !exitHere && from.Target is not ISoloMainOption and not BranchNode)
            {
                menu.AddItem(new GUIContent(DialogueNode.GetName(typeof(ExitNode))), false, () =>
                {
                    OnBeforeModify();
                    DialogueNode.Editor.SetAsExit(from.Target);
                    serializedDialog.Update();
                    if (output.connections is not null)
                    {
                        foreach (var edge in output.connections)
                        {
                            edge.input.Disconnect(edge);
                            RemoveElement(edge);
                        }
                        output.DisconnectAll();
                    }
                    AddElement(output.ConnectTo(exit.Input));
                    from.RefreshOptionButton();
                });
                menu.AddSeparator("");
            }
            foreach (var type in TypeCache.GetTypesDerivedFrom<DialogueNode>())
            {
                if (type.IsAbstract || type.IsGenericType || type.IsGenericTypeDefinition || typeof(EntryNode) == type || typeof(ExitNode) == type
                    || from.Target is BranchNode && !typeof(ConditionNode).IsAssignableFrom(type)) continue;
                var temp = Activator.CreateInstance(type) as DialogueNode;
                if (!temp.CanLinkFrom(from.userData as DialogueNode, output.Option)) continue;
                menu.AddItem(new GUIContent($"{GetMenuGroup(type)}{DialogueNode.GetName(type)}"), false, () => CreateNode(type, nodePosition, followUp));
            }
            menu.ShowAsContext();

            void followUp(DialogueGraphNode child)
            {
                DialogueOption.Editor.SetNext(option, child.Target);
                if (exitHere) SetAsExit(child);
                serializedDialog.Update();
                if (output.connections is not null)
                {
                    foreach (var edge in output.connections)
                    {
                        edge.input.Disconnect(edge);
                        RemoveElement(edge);
                    }
                    output.DisconnectAll();
                }
                AddElement(output.ConnectTo(child.Input));
                if (exitHere)
                {
                    DialogueNode.Editor.SetAsExit(from.Target, false);
                    serializedDialog.Update();
                }
            }
        }
        private void OnUndoPerformed()
        {
            List<string> selectedNodes = new List<string>();
            foreach (var item in selection)
            {
                if (item is Node node) selectedNodes.Add(node.viewDataKey);
            }
            ViewDialogue(Dialogue);
            foreach (var id in selectedNodes)
            {
                if (GetNodeByGuid(id) is Node node)
                    AddToSelection(node);
            }
        }
        private void OnDeleteOutput(DialogueOutput output)
        {
            DeleteElements(output.connections);
        }
        private void OnGroupRightClick(DialogueGroup group, ContextualMenuPopulateEvent evt)
        {
            if (selection.Count == 1)
            {
                evt.menu.AppendAction(Tr("全选"), a =>
                {
                    ClearSelection();
                    foreach (var e in group.containedElements)
                    {
                        AddToSelection(e);
                    }
                });
                if (invalid != group)
                {
                    if (group.containedElements.Any(n => n.userData is DialogueNode node && node is not EntryNode and not ExitNode))
                    {
                        evt.menu.AppendAction(Tr("复制"), a => CopySelectionCallback());
                        evt.menu.AppendAction(Tr("清空"), a => clear(group));
                        evt.menu.AppendAction(Tr("全部删除"), a => DeleteSelection());
                    }
                    evt.menu.AppendAction(Tr("仅删除组"), a =>
                    {
                        clear(group);
                        dialogue.groups.Remove(group.Group);
                        RemoveElement(group);
                    });
                }
                else evt.menu.AppendAction(Tr("全部删除"), a => DeleteSelection());
            }

            void clear(DialogueGroup group)
            {
                OnBeforeModify();
                group.Group.nodes.Clear();
                group.RemoveElementsWithoutNotification(new List<GraphElement>(group.containedElements));
            }
        }
        private void OnElementsAddedToGroup(Group group, IEnumerable<GraphElement> elements)
        {
            if (elements.Any() && group.userData is DialogueGroupData groupData)
            {
                OnBeforeModify();
                HashSet<string> exist = new HashSet<string>(groupData.nodes);
                foreach (var e in elements)
                {
                    if (!exist.Contains(e.viewDataKey))
                    {
                        (group.userData as DialogueGroupData).nodes.Add(e.viewDataKey);
                    }
                }
            }
        }
        private void OnElementsRemovedFromGroup(Group group, IEnumerable<GraphElement> elements)
        {
            if (elements.Any() && group.userData is DialogueGroupData groupData)
            {
                OnBeforeModify();
                foreach (var e in elements)
                {
                    groupData.nodes.Remove(e.viewDataKey);
                }
                if (group == invalid && group.containedElements.None())
                {
                    RemoveElement(group);
                    invalid = null;
                    ViewDialogue(dialogue);
                }
            }
        }

        private void OnBeforeModify() => Undo.RecordObject(Dialogue, Tr("修改 {0}", Dialogue.name));

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                HashSet<DialogueGraphNode> removedNodes = new HashSet<DialogueGraphNode>();
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is DialogueGraphNode node)
                    {
                        OnBeforeModify();
                        Dialogue.Editor.RemoveNode(Dialogue, node.Target);
                        removedNodes.Add(node);
                    }
                });
                if (removedNodes.Count > 0)
                    nodes.ForEach(n =>
                    {
                        if (!removedNodes.Contains(n)) (n as DialogueGraphNode).RefreshProperty();
                    });
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is Edge edge)
                    {
                        DialogueGraphNode parent = edge.output.node as DialogueGraphNode;
                        parent.Target.lastExitHere = parent.Target.ExitHere;
                        if (parent.Target.ExitHere)
                        {
                            OnBeforeModify();
                            DialogueNode.Editor.SetAsExit(parent.Target, false);
                            parent.RefreshOptionButton();
                        }
                        else
                        {
                            DialogueGraphNode child = edge.input.node as DialogueGraphNode;
                            OnBeforeModify();
                            var index = parent.Outputs.IndexOf(edge.output as DialogueOutput);
                            if (index >= 0 && index < parent.Target.Options.Count)
                                DialogueOption.Editor.SetNext(parent.Target.Options[index], null);
                        }
                    }
                    if (elem is DialogueGroup group)
                    {
                        OnBeforeModify();
                        Dialogue.groups.Remove(group.Group);
                    }
                });
            }
            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    OnBeforeModify();
                    DialogueGraphNode parent = edge.output.node as DialogueGraphNode;
                    if (edge.input.node.userData is ExitNode)
                    {
                        DialogueNode.Editor.SetAsExit(parent.Target);
                        parent.Target.lastExitHere = false;
                        parent.RefreshOptionButton();
                    }
                    else
                    {
                        var exitHere = parent.Target.lastExitHere;
                        parent.Target.lastExitHere = false;
                        DialogueNode.Editor.SetAsExit(parent.Target, false);
                        DialogueGraphNode child = edge.input.node as DialogueGraphNode;
                        if (edge.output is DialogueOutput parentOutput)
                        {
                            if (exitHere && (child.Target.Options.Count < 1 || child.Target.Options[0].IsMain && !child.Target.Options[0].Next))
                                SetAsExit(child);
                            DialogueOption.Editor.SetNext(parentOutput.Option, child.Target);
                        }
                    }
                });
            }

            if (graphViewChange.elementsToRemove != null || graphViewChange.edgesToCreate != null)
            {
                EditorUtility.SetDirty(Dialogue);
                serializedDialog.UpdateIfRequiredOrScript();
            }

            return graphViewChange;
        }

        private string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            copiedGroups = new List<DialogueGroupData>();
            copiedNodes = new List<DialogueNode>();
            copiedData = new GenericData();
            List<Vector2> positions = new List<Vector2>();
            foreach (var element in elements)
            {
                if (element is DialogueGraphNode node && node.Target is not EntryNode and not ExitNode)
                {
                    var nd = new GenericData();
                    var copy = node.Target.Copy();
                    nd["ID"] = copy.ID;
                    copiedNodes.Add(copy);
                    nd.WriteAll(node.Target.Options.Where(o => o.Next).Select(o => o.Next.ID));
                    copiedData.Write(node.viewDataKey, nd);
                    positions.Add(node.layout.center);
                }
                else if (element is DialogueGroup group)
                    copiedGroups.Add(new DialogueGroupData(group.Group.name, group.Group.position));
            }
            if (positions.Count > 0) copiedPosition = GetCenter(positions);
            else
            {
                copiedNodes = null;
                copiedData = null;
            }
            if (copiedGroups.Count < 1) copiedGroups = null;
            return "Copy";//无实际意义
        }
        private void OnUnserializeAndPaste(string operationName, string data)
        {
            if (copiedNodes != null && copiedData != null && copiedNodes.Count > 0)
            {
                OnBeforeModify();
                var offset = copiedPosition - this.ChangeCoordinatesTo(contentViewContainer, localMousePosition);
                var nodes = new List<DialogueGraphNode>();
                foreach (var copy in copiedNodes)
                {
                    Dialogue.Editor.AddNode(dialogue, copy);
                    copy._position -= offset;
                    nodes.Add(CreateGraphNode(copy));
                }
                foreach (var nd in copiedData.ReadDataDict().Values)
                {
                    var node = GetNodeByGuid(nd.ReadString("ID"));
                    if (node is DialogueGraphNode n)
                    {
                        var cds = nd.ReadStringList();
                        for (int i = 0; i < cds.Count; i++)
                        {
                            try
                            {
                                var child = GetNodeByGuid(realID(cds[i]));
                                if (child is DialogueGraphNode gn) DialogueOption.Editor.SetNext(n.Target[i], gn.Target);
                            }
                            catch { }
                        }
                    }
                }
                copiedNodes.ForEach(c => CreateEdges(c));
                if (copiedGroups != null)
                {
                    ClearSelection();
                    foreach (var group in copiedGroups)
                    {
                        group.position -= offset;
                        group.nodes = group.nodes.ConvertAll(c => realID(c));
                        dialogue.groups.Add(group);
                        AddToSelection(CreateGroup(group));
                    }
                }
                else
                {
                    ClearSelection();
                    foreach (var node in nodes)
                    {
                        AddToSelection(node);
                    }
                }
            }
            copiedGroups = null;
            copiedNodes = null;
            copiedData = null;

            string realID(string oldID) => copiedData.ReadData(oldID).ReadString("ID");
        }
        #endregion

        #region 其它
        public void ShowHideMiniMap(bool show)
        {
            if (miniMap != null) miniMap.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
        public void ViewDialogue(Dialogue newDialogue)
        {
            Vacate();
            dialogue = newDialogue;
            if (dialogue)
            {
                serializedDialog = new SerializedObject(Dialogue);
                SerializedNodes = serializedDialog.FindProperty("nodes");
                SerializedGroups = serializedDialog.FindProperty("groups");
                exit = CreateGraphNode(Dialogue.exit);
                List<DialogueNode> invalid = new List<DialogueNode>(dialogue.Nodes.Where(x => x is null));
                if (invalid.Count > 0)
                {
                    AddElement(this.invalid = new DialogueGroup(null, null, OnGroupRightClick, OnBeforeModify));
                    this.invalid.title = "无效的结点";
                    var row = -1;
                    for (int i = 0; i < invalid.Count; i++)
                    {
                        if (i % 5 == 0) row++;
                        var n = CreateGraphNode(invalid[i]);
                        n.SetPosition(new Rect(new Vector2(i % 5 * 235f, row * 40f), Vector2.zero));
                        this.invalid.AddElement(n);
                    }
                }
                else
                {
                    dialogue.Nodes.ForEach(c => CreateGraphNode(c));
                    dialogue.Nodes.ForEach(c => CreateEdges(c));
                    dialogue.groups.ForEach(g => CreateGroup(g));
                }
            }
            else
            {
                entry = null;
                exit = null;
                invalid = null;
                serializedDialog = null;
                SerializedNodes = null;
                SerializedGroups = null;
            }
        }
        private void Vacate()
        {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;
        }
        public void CheckErrors()
        {
            errors.Clear();
            if (Dialogue)
            {

                if (!Dialogue.Exitable)
                {
                    var label = new Label(Utility.ColorText($"{Tr("错误")}: {Tr("对话无结束点")}", Color.red)) { enableRichText = true };
                    label.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        EditorApplication.delayCall += () =>
                        {
                            ClearSelection();
                            AddToSelection(exit);
                            FrameSelection();
                        };
                    });
                    errors.Add(label);
                }
                if (Dialogue.Traverse(dialogue.Entry, n => n.Options.Count > 0 && n.Options.All(x => x.Next is DecoratorNode && x.Next.Exitable)))
                    errors.Add(new Label(Utility.ColorText($"{Tr("警告")}: {Tr("对话可能无结束点")}", Color.yellow)) { enableRichText = true });
                for (int i = 0; i < dialogue.Nodes.Count; i++)
                {
                    var node = dialogue.Nodes[i];
                    if (dialogue.Reachable(node))
                    {
                        if (!node.IsValid)
                        {
                            var label = new Label(Utility.ColorText($"{Tr("错误")}: {Tr("第{0}个内容填写错误", i)}", Color.red)) { enableRichText = true };
                            label.RegisterCallback<PointerDownEvent>(evt =>
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    ClearSelection();
                                    AddToSelection(GetNodeByGuid(node.ID));
                                    FrameSelection();
                                };
                            });
                            errors.Add(label);
                        }
                        if (!node.ExitHere && node.Options.Any(x => x.Next is null))
                        {
                            var label = new Label(Utility.ColorText($"{Tr("错误")}: {Tr("第{0}个内容存在无效选项", i)}", Color.red)) { enableRichText = true };
                            label.RegisterCallback<PointerDownEvent>(evt =>
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    ClearSelection();
                                    AddToSelection(GetNodeByGuid(node.ID));
                                    FrameSelection();
                                };
                            });
                            errors.Add(label);
                        }
                    }
                }
            }
        }
        private void SetAsExit(DialogueGraphNode node)
        {
            if (node.Target is SuffixNode || node.Target is ExternalOptionsNode) return;
            DialogueNode.Editor.SetAsExit(node.Target);
            serializedDialog.UpdateIfRequiredOrScript();
            DialogueOutput output = node.Outputs.Count < 1 ? node.AddOutput(node.Target.Options[0]) : node.Outputs[0];
            AddElement(output.ConnectTo(exit.Input));
            node.RefreshOptionButton();
        }
        private static string GetMenuGroup(Type type)
        {
            string group = DialogueNode.GetGroup(type);
            if (string.IsNullOrEmpty(group)) return group;
            else return group.EndsWith("/") ? group : (group + "/");
        }
        private Vector2 GetCenter(IEnumerable<Vector2> positions)
        {
            Vector2 min = new Vector2(positions.Min(p => p.x), positions.Min(p => p.y));
            Vector2 max = new Vector2(positions.Max(p => p.x), positions.Max(p => p.y));
            return Utility.CenterBetween(min, max);
        }
        #endregion

        private string Tr(string text) => L.Tr(settings.language, text);
        private string Tr(string text, params object[] args) => L.Tr(settings.language, text, args);
    }
}