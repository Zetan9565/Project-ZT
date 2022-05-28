using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZetanStudio.Extension;

namespace ZetanStudio.BehaviourTree.Editor
{
    public sealed class BehaviourTreeEditor : EditorWindow
    {
        #region 视图相关
        private BehaviourTreeView treeView;
        private BehaviourTree tree;
        private Label inspectorLabel;
        private InspectorView inspectorView;
        private TabbedBar inspectorTaber;
        private TabbedBar variableTaber;
        private IMGUIContainer variables;
        private ToolbarMenu assetsMenu;
        private ToolbarMenu exeMenu;
        private ToolbarButton undo;
        private ToolbarButton redo;
        private Label treeName;
        private GameObject latestGo;
        private GameObject goBefPlay;
        private BehaviourTreeEditorSettings settings;
        #endregion

        #region 变量相关
        private bool showInspector;
        private bool showShared;
        private SerializedObject serializedTree;
        private SerializedObject serializedGlobal;
        private SerializedObject serializedObject;
        private SerializedProperty serializedVariables;
        private SharedVariableListDrawer variableList;
        #endregion

        #region 静态回调
        [MenuItem("Zetan Studio/行为树编辑器")]
        public static void CreateWindow()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>(Language.Tr(BehaviourTreeEditorSettings.GetOrCreate().language, "行为树编辑器"));
            wnd.minSize = BehaviourTreeEditorSettings.GetOrCreate().minWindowSize;
        }
        public static void CreateWindow(BehaviourExecutor executor)
        {
            if (!executor) return;
            bool isOpen = HasOpenInstances<BehaviourTreeEditor>();
            EditorGUIUtility.PingObject(executor);
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>(Language.Tr(BehaviourTreeEditorSettings.GetOrCreate().language, "行为树编辑器"));
            wnd.minSize = BehaviourTreeEditorSettings.GetOrCreate().minWindowSize;
            Selection.activeGameObject = executor.gameObject;
            if (isOpen) wnd.ChangeTreeBySelection(executor);
        }
        public static void CreateWindow(BehaviourTree tree)
        {
            if (!tree) return;
            bool isOpen = HasOpenInstances<BehaviourTreeEditor>();
            Selection.activeObject = tree;
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>(Language.Tr(BehaviourTreeEditorSettings.GetOrCreate().language, "行为树编辑器"));
            wnd.minSize = BehaviourTreeEditorSettings.GetOrCreate().minWindowSize;
            if (isOpen) wnd.ChangeTreeBySelection();
        }

        [OnOpenAsset]
#pragma warning disable IDE0060 // 删除未使用的参数
        public static bool OnOpenAsset(int instanceID, int line)
#pragma warning restore IDE0060 // 删除未使用的参数
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is BehaviourTree tree)
            {
                CreateWindow(tree);
                return true;
            }
            return false;
        }
        #endregion

        #region 状态变化相关
        private void SelectTree(BehaviourTree selected)
        {

            if (treeView == null || !selected)
            {
                UpdateVariables();
                return;
            }

            tree = selected;

            treeView.DrawTreeView(tree);

            serializedTree = new SerializedObject(tree);
            UpdateVariables();
            UpdateAssetDropdown();
            UpdateTreeDropdown();
            UpdateTreeName();
            UpdateUndoVisible();
            UpdateUndoEnable();
            OnInspectorTab(inspectorTaber.SelectedIndex);

            EditorApplication.delayCall += () =>
            {
                treeView.FrameAll();
                inspectorView.InspectTree(tree);
            };

            void UpdateVariables()
            {
                serializedGlobal = new SerializedObject(GetGlobal());
                InitVariables();
            }
        }

        private GlobalVariables GetGlobal()
        {
            GlobalVariables global;
            if (Application.isPlaying && (!tree || tree.IsInstance)) global = BehaviourManager.Instance.GlobalVariables;
            else global = ZetanUtility.Editor.LoadAsset<GlobalVariables>();
            return global;
        }

        private void UpdateAssetDropdown()
        {
            if (assetsMenu == null) return;
            settings = settings ? settings : BehaviourTreeEditorSettings.GetOrCreate();
            var behaviourTrees = ZetanUtility.Editor.LoadAssets<BehaviourTree>();
            for (int i = assetsMenu.menu.MenuItems().Count - 1; i > 0; i--)
            {
                assetsMenu.menu.RemoveItemAt(i);
            }
            if (!Application.isPlaying && tree && tree.IsRuntime) assetsMenu.menu.InsertAction(1, Tr("保存到本地"), (a) => { SaveToLocal("new behaviour tree"); });
            if (behaviourTrees.Count > 0) assetsMenu.menu.AppendSeparator();
            int counter = 1;
            behaviourTrees.ForEach(tree =>
            {
                if (tree)
                {
                    assetsMenu.menu.AppendAction($"{Tr("本地")}/[{counter}] {tree.name} ({ZetanUtility.Editor.GetDirectoryName(tree).Replace("\\", "/").Replace("Assets/", "").Replace("/", "\u2215")})", (a) =>
                    {
                        EditorApplication.delayCall += () =>
                        {
                            latestGo = null;
                            SelectTree(tree);
                        };
                    });
                    counter++;
                }
            });
            counter = 1;
            foreach (var exe in FindObjectsOfType<BehaviourExecutor>(true))
                if (exe.Behaviour)
                {
                    assetsMenu.menu.AppendAction($"{Tr("场景")}/[{counter}] {exe.gameObject.name} ({exe.gameObject.GetPath().Replace("/", "\u2215")})", (a) =>
                    {
                        EditorApplication.delayCall += () =>
                        {
                            latestGo = exe.gameObject;
                            SelectTree(exe.Behaviour);
                        };
                    });
                    counter++;
                }
        }
        private void UpdateTreeView()
        {
            if (tree && latestGo)
            {
                BehaviourExecutor[] exes = latestGo.GetComponents<BehaviourExecutor>();
                if (!Array.Exists(exes, e => e.Behaviour == tree))
                {
                    if (tree.IsRuntime)
                    {
                        tree = null;
                        treeView.Vocate();
                    }
                    else if (exes.Length > 0) ChangeTreeBySelection();
                    else if (!treeView.tree) treeView.DrawTreeView(tree);
                }
                else if (!treeView.tree) treeView.DrawTreeView(tree);
            }
            else if (!treeView.tree) treeView.DrawTreeView(tree);
            treeView.OnUpdate();
        }
        private void UpdateTreeDropdown()
        {
            if (exeMenu != null)
                if (tree && latestGo && Array.Exists(latestGo.GetComponents<BehaviourExecutor>(), e => e.Behaviour == tree))
                {
                    for (int i = exeMenu.menu.MenuItems().Count - 1; i >= 0; i--)
                    {
                        exeMenu.menu.RemoveItemAt(i);
                    }
                    BehaviourExecutor[] executors = latestGo.GetComponents<BehaviourExecutor>();
                    for (int i = 0; i < executors.Length; i++)
                    {
                        var exe = executors[i];
                        if (exe.Behaviour)
                            exeMenu.menu.AppendAction($"{(tree == exe.Behaviour ? "√" : "  ")} [{i + 1}] {exe.Behaviour.Name}",
                                (a) => SelectTree(exe.Behaviour));
                    }
                    exeMenu.visible = exeMenu.menu.MenuItems().Count > 0;
                    if (tree && exeMenu.visible) exeMenu.text = tree.Name;
                }
                else exeMenu.visible = false;
        }
        private void UpdateTreeName()
        {
            if (treeName != null)
            {
                treeName.text = Tr("行为树视图");
                if (tree)
                {
                    if (latestGo)
                    {
                        foreach (var exe in latestGo.GetComponents<BehaviourExecutor>())
                        {
                            if (exe.Behaviour == tree)
                            {
                                treeName.text = $"{Tr("行为树视图")}\t{Tr("当前")}：{tree.Name} ({exe.gameObject.GetPath()} <{exe.GetType().Name}.{ZetanUtility.GetMemberName(() => exe.Behaviour)}>)";
                                return;
                            }
                        }
                    }
                    if (!tree.IsRuntime) treeName.text = $"{Tr("行为树视图")}\t{Tr("当前")}：{tree.Name} ({AssetDatabase.GetAssetPath(tree)})";
                }
            }
        }
        private void UpdateUndoVisible()
        {
            if (undo == null || redo == null) return;
            bool isLocal = ZetanUtility.Editor.IsLocalAssets(tree);
            undo.visible = treeView != null && tree && !isLocal;
            redo.visible = treeView != null && tree && !isLocal;
        }
        private void UpdateUndoEnable()
        {
            if (undo == null || redo == null) return;
            if (treeView != null)
            {
                undo.SetEnabled(treeView.CanUndo);
                undo.tooltip = treeView.UndoName;
                redo.SetEnabled(treeView.CanRedo);
                redo.tooltip = treeView.RedoName;
            }
        }

        private void OnUndoClick()
        {
            treeView?.UndoOperation();
            UpdateUndoEnable();
        }
        private void OnRedoClick()
        {
            treeView?.RedoOperation();
            UpdateUndoEnable();
        }

        private void OnNodeSelected(NodeEditor selected)
        {
            if (!showInspector) return;
            var nodesSelected = treeView.nodes.ToList().FindAll(x => x.selected);
            if (nodesSelected.Count > 1)
            {
                inspectorView.InspectMultSelect(nodesSelected.ConvertAll(x => x as NodeEditor));
            }
            else inspectorView?.InspectNode(tree, selected);
        }
        private void OnNodeUnselected(NodeEditor unseleted)
        {
            if (!showInspector) return;
            if (inspectorView != null && inspectorView.nodeEditor == unseleted)
                inspectorView.Clear();
            if (!treeView.nodes.ToList().Exists(x => x.selected))
                inspectorView?.InspectTree(tree);
        }
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    if (goBefPlay) ChangeTreeBySelection(goBefPlay.GetComponent<BehaviourExecutor>());
                    else ChangeTreeBySelection();
                    UpdateUndoVisible();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    goBefPlay = latestGo;
                    ChangeTreeBySelection();
                    UpdateUndoVisible();
                    break;
            }
        }
        private void OnPauseStateChanged(PauseState state)
        {
            ChangeTreeBySelection();
        }
        private void ChangeTreeBySelection()
        {
            EditorApplication.delayCall += () =>
            {
                BehaviourTree tree = Selection.activeObject as BehaviourTree;
                if (!tree)
                {
                    if (Selection.activeGameObject)
                    {
                        BehaviourExecutor exe = Selection.activeGameObject.GetComponent<BehaviourExecutor>();
                        if (exe)
                        {
                            tree = exe.Behaviour;
                            latestGo = exe.gameObject;
                        }
                    }
                }
                else latestGo = null;
                SelectTree(tree);
            };
        }
        private void ChangeTreeBySelection(BehaviourExecutor exe)
        {
            EditorApplication.delayCall += () =>
            {
                if (exe)
                {
                    tree = exe.Behaviour;
                    latestGo = exe.gameObject;
                }
                else latestGo = null;
                SelectTree(tree);
            };
        }
        private void OnInspectorTab(int index)
        {
            switch (index)
            {
                case 1:
                    showInspector = true;
                    break;
                case 2:
                    showInspector = false;
                    break;
                default:
                    break;
            }
            if (inspectorLabel != null) inspectorLabel.text = showInspector ? Tr("检查器") : Tr("结点类型列表");
            if (inspectorView != null && treeView != null)
                if (showInspector)
                {
                    var nodesSelected = treeView.nodes.ToList().FindAll(x => x.selected);
                    if (nodesSelected.Count > 1)
                    {
                        inspectorView.InspectMultSelect(nodesSelected.ConvertAll(x => x as NodeEditor));
                    }
                    else if (nodesSelected.Count > 0) inspectorView.InspectNode(tree, nodesSelected[0] as NodeEditor);
                    else inspectorView.InspectTree(tree);
                }
                else inspectorView.InspectNodes(treeView.InsertNode);
        }
        private void OnVariableTab(int index)
        {
            switch (index)
            {
                case 1:
                    showShared = true;
                    InitVariables();
                    break;
                case 2:
                    showShared = false;
                    InitVariables();
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Unity回调
        public void CreateGUI()
        {
            try
            {
                settings = settings ? settings : BehaviourTreeEditorSettings.GetOrCreate();

                VisualElement root = rootVisualElement;

                var visualTree = settings.treeUxml;
                visualTree.CloneTree(root);

                var styleSheet = settings.treeUss;
                root.styleSheets.Add(styleSheet);

                treeView = root.Q<BehaviourTreeView>();
                treeView.nodeSelectedCallback = OnNodeSelected;
                treeView.nodeUnselectedCallback = OnNodeUnselected;
                treeView.undoRecordsChangedCallback = UpdateUndoEnable;

                inspectorTaber = root.Q<TabbedBar>("inspector-tab");
                inspectorTaber.Refresh(Language.TrM(settings.language, "检查器", "快速插入"), OnInspectorTab, 4);
                inspectorLabel = root.Q<Label>("inspector-label");
                showInspector = true;
                inspectorView = root.Q<InspectorView>();
                variables = root.Q<IMGUIContainer>("variables");
                variables.onGUIHandler = DrawVariables;

                assetsMenu = root.Q<ToolbarMenu>("assets");
                assetsMenu.text = Tr("文件");
                assetsMenu.menu.AppendAction(Tr("新建"), (a) => CreateNewTree("new behaviour tree"));
                exeMenu = root.Q<ToolbarMenu>("exe-select");
                UpdateAssetDropdown();

                treeName = root.Q<Label>("tree-name");

                root.Q<Label>("variable-title").text = Tr("变量");
                variableTaber = root.Q<TabbedBar>("variable-tab");
                variableTaber.Refresh(Language.TrM(settings.language, "共享变量", "全局变量").ToArray(), OnVariableTab, 4);
                serializedGlobal = new SerializedObject(Application.isPlaying && BehaviourManager.Instance ? BehaviourManager.Instance.GlobalVariables : ZetanUtility.Editor.LoadAsset<GlobalVariables>());

                undo = root.Q<ToolbarButton>("undo");
                undo.clicked += OnUndoClick;
                redo = root.Q<ToolbarButton>("redo");
                redo.clicked += OnRedoClick;
                UpdateUndoVisible();
                UpdateUndoEnable();

                inspectorTaber.SetSelected(1);
                variableTaber.SetSelected(1);

                if (tree == null) ChangeTreeBySelection();
                else SelectTree(tree);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        private void OnSelectionChange()
        {
            settings = settings ? settings : BehaviourTreeEditorSettings.GetOrCreate();
            if (settings.changeOnSelected) ChangeTreeBySelection();
        }
        private void OnInspectorUpdate()
        {
            if (treeView != null) UpdateTreeView();
            UpdateTreeDropdown();
            UpdateTreeName();
        }
        private void OnProjectChange()
        {
            UpdateAssetDropdown();
        }
        private void OnHierarchyChange()
        {
            UpdateAssetDropdown();
        }
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.pauseStateChanged += OnPauseStateChanged;
        }
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        private void OnDestroy()
        {
            if (tree)
            {
                EditorUtility.SetDirty(tree);
                Undo.ClearUndo(tree);
            }
        }
        #endregion

        #region 变量相关
        private void InitVariables()
        {
            if (showShared)
            {
                serializedObject = serializedTree;
                if (serializedObject != null) serializedVariables = serializedObject.FindProperty("variables");
                else serializedVariables = null;
            }
            else
            {
                serializedObject = serializedGlobal;
                if (serializedObject != null) serializedVariables = serializedObject.FindProperty("variables");
                else serializedVariables = null;
            }
            InitVariableList();
        }
        private void InitVariableList()
        {
            if (serializedObject == null || !serializedObject.targetObject || serializedVariables == null) return;
            variableList = new SharedVariableListDrawer(serializedVariables, showShared);
        }
        private void DrawVariables()
        {
            if (serializedObject == null || serializedObject.targetObject == null)
            {
                if (!showShared)
                {
                    if (!Application.isPlaying || tree && !tree.IsInstance)
                    {
                        if (GUILayout.Button(Tr("新建"))) CreateGlobalVariables("global variables");
                    }
                    else EditorGUILayout.HelpBox(Tr("没有可用的全局变量"), MessageType.Info);
                }
                else EditorGUILayout.HelpBox(Tr("没有编辑中的行为树"), MessageType.Info);
                return;
            }
            if (serializedObject.targetObject)
            {
                serializedObject.UpdateIfRequiredOrScript();
                variableList?.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion

        #region 新建相关
        private void CreateNewTree(string assetName)
        {
            BehaviourTree tree = ZetanUtility.Editor.SaveFilePanel(CreateInstance<BehaviourTree>, assetName, folder: settings.newAssetFolder, ping: true, select: true);
            if (tree)
            {
                serializedTree = new SerializedObject(tree);
                InitVariables();
                Selection.activeObject = tree;
                EditorGUIUtility.PingObject(tree);
                settings = settings ? settings : BehaviourTreeEditorSettings.GetOrCreate();
                if (!settings.changeOnSelected) ChangeTreeBySelection();
            }
        }
        private void CreateGlobalVariables(string assetName)
        {
            if (serializedGlobal != null && serializedGlobal.targetObject != null) return;
            GlobalVariables global = ZetanUtility.Editor.SaveFilePanel(CreateInstance<GlobalVariables>, assetName, folder: settings.newAssetFolder);
            if (global)
            {
                serializedGlobal = new SerializedObject(global);
                InitVariables();
            }
        }
        private void SaveToLocal(string assetName)
        {
            if (!tree || Application.isPlaying) return;
            if (!tree.IsRuntime)
            {
                EditorUtility.DisplayDialog(Tr("保存失败"), Tr("无需保存，已经是本地行为树"), Tr("确定"));
                return;
            }
            if (EditorUtility.DisplayDialog(Tr("保存到本地"), Tr("保存到本地的行为树将失去对场景对象的引用，是否继续？"), Tr("继续"), Tr("取消")))
            {
                ZetanUtility.Editor.SaveFilePanel(() => BehaviourTree.PrepareLocalization(tree), assetName, ping: true);
            }
        }
        #endregion

        private string Tr(string text, params object[] args)
        {
            return Language.Tr(settings.language, text, args);
        }
        private string Tr(string text)
        {
            return Language.Tr(settings.language, text);
        }
    }
}