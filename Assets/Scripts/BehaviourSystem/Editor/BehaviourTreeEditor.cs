using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZetanExtends;

namespace ZetanStudio.BehaviourTree
{
    public sealed class BehaviourTreeEditor : EditorWindow
    {
        #region 视图相关
        private BehaviourTreeView treeView;
        private BehaviourTree tree;
        private Label inspectorLabel;
        private InspectorView inspectorView;
        private Button inspector;
        private Button searcher;
        private IMGUIContainer variables;
        private ToolbarMenu assetsMenu;
        private ToolbarMenu exeMenu;
        private ToolbarButton undo;
        private ToolbarButton redo;
        private Label treeName;
        private GameObject latestGo;

        private BehaviourTreeSettings settings;
        #endregion

        #region 变量相关
        private Button shared;
        private Button global;
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
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("行为树编辑器");
        }
        public static void CreateWindow(BehaviourExecutor executor)
        {
            if (!executor) return;
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("行为树编辑器");
            Selection.activeGameObject = executor.gameObject;
            EditorGUIUtility.PingObject(executor);
            wnd.ChangeTreeBySelection(executor);
        }
        public static void CreateWindow(BehaviourTree tree)
        {
            if (!tree) return;
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("行为树编辑器");
            Selection.activeObject = tree;
            wnd.ChangeTreeBySelection();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is BehaviourTree tree)
            {
                CreateWindow(tree);
                return true;
            }
            return false;
        }
        #endregion

        #region 按钮点击
        private void OnInspectorClick()
        {
            showInspector = true;
            SwitchInspector();
        }
        private void OnSeacherClick()
        {
            showInspector = false;
            SwitchInspector();
        }
        private void OnGlobalClick()
        {
            showShared = false;
            UpdateVariables();
        }
        private void OnSharedClick()
        {
            showShared = true;
            UpdateVariables();
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
            SwitchInspector();

            EditorApplication.delayCall += () =>
            {
                treeView.FrameAll();
                inspectorView.InspectTree(tree);
            };

            void UpdateVariables()
            {
                serializedGlobal = new SerializedObject(Application.isPlaying && BehaviourManager.Instance ? BehaviourManager.Instance.GlobalVariables : ZetanEditorUtility.LoadAsset<GlobalVariables>());
                this.UpdateVariables();
            }
        }
        private void UpdateAssetDropdown()
        {
            if (assetsMenu == null) return;
            settings = settings ? settings : BehaviourTreeSettings.GetOrCreate();
            var behaviourTrees = ZetanEditorUtility.LoadAssets<BehaviourTree>();
            for (int i = assetsMenu.menu.MenuItems().Count - 1; i > 0; i--)
            {
                assetsMenu.menu.RemoveItemAt(i);
            }
            if (!Application.isPlaying && tree && tree.IsRuntime) assetsMenu.menu.InsertAction(1, "保存到本地", (a) => { SaveToLocal("new behaviour tree"); });
            if (behaviourTrees.Count > 0) assetsMenu.menu.AppendSeparator();
            int counter = 1;
            behaviourTrees.ForEach(tree =>
            {
                if (tree)
                {
                    assetsMenu.menu.AppendAction($"本地/[{counter}] {tree.name} ({ZetanEditorUtility.GetDirectoryName(tree).Replace("\\", "/").Replace("Assets/", "").Replace("/", "\u2215")})", (a) =>
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
                    assetsMenu.menu.AppendAction($"场景/[{counter}] {exe.gameObject.name} ({exe.gameObject.GetPath().Replace("/", "\u2215")})", (a) =>
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
                if (!System.Array.Exists(exes, e => e.Behaviour == tree))
                {
                    if (tree.IsRuntime)
                    {
                        tree = null;
                        treeView.Vocate();
                    }
                    else if (exes.Length > 0)
                    {
                        ChangeTreeBySelection();
                    }
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
                if (tree && latestGo && System.Array.Exists(latestGo.GetComponents<BehaviourExecutor>(), e => e.Behaviour == tree))
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
                            exeMenu.menu.AppendAction($"[{i + 1}] {(string.IsNullOrEmpty(exe.Behaviour.Name) ? "(未命名)" : exe.Behaviour.Name)}{(tree == exe.Behaviour ? "\t(当前)" : string.Empty)}",
                                (a) => SelectTree(exe.Behaviour));
                    }
                    exeMenu.visible = exeMenu.menu.MenuItems().Count > 0;
                    if (tree && exeMenu.visible) exeMenu.text = string.IsNullOrEmpty(tree.Name) ? "(未命名)" : tree.Name;
                }
                else
                {
                    exeMenu.visible = false;
                }
        }
        private void UpdateVariables()
        {
            shared?.SetEnabled(!showShared);
            global?.SetEnabled(showShared);
            InitVariables();
        }
        private void UpdateTreeName()
        {
            if (treeName != null)
            {
                treeName.text = "行为树视图";
                if (tree)
                {
                    if (latestGo)
                    {
                        foreach (var exe in latestGo.GetComponents<BehaviourExecutor>())
                        {
                            if (exe.Behaviour == tree)
                            {
                                treeName.text = $"行为树视图\t当前：{(string.IsNullOrEmpty(tree.Name) ? "(未命名)" : tree.Name)} ({exe.gameObject.GetPath()} <{exe.GetType().Name}.{ZetanUtility.GetMemberName(() => exe.Behaviour)}>)";
                                return;
                            }
                        }
                    }
                    if (!tree.IsRuntime) treeName.text = $"行为树视图\t当前：{(string.IsNullOrEmpty(tree.Name) ? "(未命名)" : tree.Name)} ({AssetDatabase.GetAssetPath(tree)})";
                }
            }
        }
        private void UpdateUndoRedo()
        {
            if (undo == null || redo == null) return;
            undo.visible = treeView != null && Application.isPlaying;
            redo.visible = treeView != null && Application.isPlaying;
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
            UpdateUndoRedo();
        }
        private void OnRedoClick()
        {
            treeView?.RedoOperation();
            UpdateUndoRedo();
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
        private GameObject goBefPlay;
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    if (goBefPlay) ChangeTreeBySelection(goBefPlay.GetComponent<BehaviourExecutor>());
                    else ChangeTreeBySelection();
                    UpdateUndoRedo();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    goBefPlay = latestGo;
                    ChangeTreeBySelection();
                    UpdateUndoRedo();
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
        private void SwitchInspector()
        {
            inspectorLabel.text = showInspector ? "检查器" : "结点类型列表";
            inspector.SetEnabled(!showInspector);
            searcher.SetEnabled(showInspector && tree);
            if (showInspector)
            {
                var nodesSelected = treeView.nodes.ToList().FindAll(x => x.selected);
                if (nodesSelected.Count > 1)
                {
                    inspectorView.InspectMultSelect(nodesSelected.ConvertAll(x => x as NodeEditor));
                }
                else if (nodesSelected.Count > 0) inspectorView?.InspectNode(tree, nodesSelected[0] as NodeEditor);
                else inspectorView?.InspectTree(tree);
            }
            else inspectorView.InspectNodes(treeView.InsertNode);
        }
        #endregion

        #region Unity回调
        public void CreateGUI()
        {
            settings = settings ? settings : BehaviourTreeSettings.GetOrCreate();

            VisualElement root = rootVisualElement;

            var visualTree = settings.treeUxml;
            visualTree.CloneTree(root);

            var styleSheet = settings.treeUss;
            root.styleSheets.Add(styleSheet);

            treeView = root.Q<BehaviourTreeView>();
            treeView.nodeSelectedCallback = OnNodeSelected;
            treeView.nodeUnselectedCallback = OnNodeUnselected;
            treeView.undoRecordsChangedCallback = UpdateUndoRedo;

            inspectorLabel = root.Q<Label>("inspector-label");
            inspector = root.Q<Button>("view-inspector");
            inspector.clicked += OnInspectorClick;
            searcher = root.Q<Button>("search-node");
            searcher.clicked += OnSeacherClick;
            showInspector = true;
            inspectorView = root.Q<InspectorView>();
            SwitchInspector();
            variables = root.Q<IMGUIContainer>("variables");
            variables.onGUIHandler = DrawVariables;

            assetsMenu = root.Q<ToolbarMenu>("assets");
            assetsMenu.menu.AppendAction("新建", (a) => CreateNewTree("new behaviour tree"));
            exeMenu = root.Q<ToolbarMenu>("exe-select");
            UpdateAssetDropdown();

            treeName = root.Q<Label>("tree-name");

            shared = root.Q<Button>("shared");
            shared.clicked += OnSharedClick;
            global = root.Q<Button>("global");
            global.clicked += OnGlobalClick;
            showShared = true;
            serializedGlobal = new SerializedObject(Application.isPlaying && BehaviourManager.Instance ? BehaviourManager.Instance.GlobalVariables : ZetanEditorUtility.LoadAsset<GlobalVariables>());
            UpdateVariables();

            undo = root.Q<ToolbarButton>("undo");
            undo.clicked += OnUndoClick;
            redo = root.Q<ToolbarButton>("redo");
            redo.clicked += OnRedoClick;
            UpdateUndoRedo();

            if (tree == null) ChangeTreeBySelection();
            else SelectTree(tree);
        }
        private void OnSelectionChange()
        {
            settings = settings ? settings : BehaviourTreeSettings.GetOrCreate();
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
            ChangeTreeBySelection();
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
            variableList = new SharedVariableListDrawer(serializedObject, serializedVariables, showShared);
        }
        private void DrawVariables()
        {
            if (serializedObject == null || serializedObject.targetObject == null)
            {
                if (!showShared)
                {
                    if (GUILayout.Button("新建")) CreateGlobalVariables("global variables");
                }
                else EditorGUILayout.HelpBox("没有编辑中的行为树", MessageType.Info);
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
            BehaviourTree tree = ZetanEditorUtility.SaveFilePanel(CreateInstance<BehaviourTree>, assetName, ping: true, select: true);
            if (tree)
            {
                serializedTree = new SerializedObject(tree);
                UpdateVariables();
                Selection.activeObject = tree;
                EditorGUIUtility.PingObject(tree);
                settings = settings ? settings : BehaviourTreeSettings.GetOrCreate();
                if (!settings.changeOnSelected) ChangeTreeBySelection();
            }
        }
        private void CreateGlobalVariables(string assetName)
        {
            if (serializedGlobal != null && serializedGlobal.targetObject != null) return;
            GlobalVariables global = ZetanEditorUtility.SaveFilePanel(CreateInstance<GlobalVariables>, assetName);
            if (global)
            {
                serializedGlobal = new SerializedObject(global);
                UpdateVariables();
            }
        }
        private void SaveToLocal(string assetName)
        {
            if (!tree || Application.isPlaying) return;
            if (!tree.IsRuntime)
            {
                EditorUtility.DisplayDialog("保存失败", "无需保存，已经是本地行为树", "确定");
                return;
            }
            if (EditorUtility.DisplayDialog("保存到本地", "保存到本地的行为树将失去对场景对象的引用，是否继续？", "继续", "取消"))
            {
                ZetanEditorUtility.SaveFilePanel(() => BehaviourTree.PrepareLocalization(tree), assetName, ping: true);
            }
        }
        #endregion
    }
}