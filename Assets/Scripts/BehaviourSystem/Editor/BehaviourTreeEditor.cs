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
        private InspectorView inspectorView;
        private IMGUIContainer variables;
        private ToolbarMenu assetsMenu;
        private ToolbarMenu exeMenu;
        private ToolbarButton undo;
        private ToolbarButton redo;
        //private ToolbarSearchField searchField;
        private Label treeName;
        #endregion

        #region 变量相关
        private Button shared;
        private Button global;
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
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("行为树编辑器");
            Selection.activeGameObject = executor.gameObject;
            EditorGUIUtility.PingObject(executor);
            wnd.OnSelectionChange();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is BehaviourTree)
            {
                CreateWindow();
                return true;
            }
            return false;
        }
        #endregion

        #region 按钮点击
        private void OnGlobalClick()
        {
            showShared = false;
            CheckShowShared();
        }
        private void OnSharedClick()
        {
            showShared = true;
            CheckShowShared();
        }
        private void OnUndoClick()
        {
            treeView?.UndoOperation();
            CheckUndoRedo();
        }
        private void OnRedoClick()
        {
            treeView?.RedoOperation();
            CheckUndoRedo();
        }
        #endregion

        #region 状态变化相关
        private void SelectTree(BehaviourTree selected)
        {

            if (treeView == null || !selected) return;
            tree = selected;

            treeView.DrawTreeView(tree);

            serializedTree = new SerializedObject(tree);
            var global = Application.isPlaying && BehaviourManager.Instance ? BehaviourManager.Instance.GlobalVariables : ZetanEditorUtility.LoadAsset<GlobalVariables>();
            serializedGlobal = new SerializedObject(global);
            CheckShowShared();
            CheckAssetDropdown();

            EditorApplication.delayCall += () =>
            {
                treeView.FrameAll();
                inspectorView.InspectTree(tree);
            };
        }
        private void CheckAssetDropdown()
        {
            if (assetsMenu == null) return;
            var behaviourTrees = ZetanEditorUtility.LoadAssets<BehaviourTree>();
            for (int i = assetsMenu.menu.MenuItems().Count - 1; i > 0; i--)
            {
                assetsMenu.menu.RemoveItemAt(i);
            }
            if (!Application.isPlaying && tree && tree.IsRuntime) assetsMenu.menu.InsertAction(1, "保存到本地", (a) => { SaveToLocal("new behaviour tree"); });
            if (behaviourTrees.Count > 0) assetsMenu.menu.AppendSeparator();
            behaviourTrees.ForEach(tree =>
            {
                if (tree)
                    assetsMenu.menu.AppendAction($"本地/{tree.name} ({ZetanEditorUtility.GetDirectoryName(tree).Replace("\\", "/").Replace("Assets/", "").Replace("/", "\u2215")})", (a) =>
                     {
                         Selection.activeObject = tree;
                     });
            });
            foreach (var exe in FindObjectsOfType<BehaviourExecutor>())
                if (exe.Behaviour)
                    assetsMenu.menu.AppendAction($"场景/{exe.gameObject.name} ({exe.gameObject.GetPath().Replace("/", "\u2215")})", (a) =>
                    {
                        Selection.activeObject = exe;
                    });
        }
        private void CheckExeDropdown()
        {
            if (exeMenu != null)
                if (Selection.activeGameObject)
                {
                    for (int i = exeMenu.menu.MenuItems().Count - 1; i >= 0; i--)
                    {
                        exeMenu.menu.RemoveItemAt(i);
                    }
                    BehaviourExecutor[] executors = Selection.activeGameObject.GetComponents<BehaviourExecutor>();
                    foreach (var exe in executors)
                    {
                        if (exe.Behaviour)
                            exeMenu.menu.AppendAction(string.IsNullOrEmpty(exe.Behaviour.Name) ? "未命名" : exe.Behaviour.Name, (a) => SelectTree(exe.Behaviour));
                    }
                    exeMenu.visible = exeMenu.menu.MenuItems().Count > 0;
                }
        }
        private void CheckShowShared()
        {
            shared.SetEnabled(!showShared);
            global.SetEnabled(showShared);
            InitVariables();
            variables.onGUIHandler = DrawVariables;
        }
        private void CheckUndoRedo()
        {
            if (treeView != null)
            {
                undo.SetEnabled(treeView.CanUndo());
                redo.SetEnabled(treeView.CanRedo());
            }
        }
        private void OnNodeSelected(NodeEditor selected)
        {
            var nodesSelected = treeView.nodes.ToList().FindAll(x => x.selected);
            if (nodesSelected.Count > 1)
            {
                inspectorView.InspectMultSelect(nodesSelected.ConvertAll(x => x as NodeEditor));
            }
            else inspectorView?.InspectNode(tree, selected);
        }
        private void OnNodeUnselected(NodeEditor unseleted)
        {
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
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
            }
        }
        #endregion

        #region Unity回调
        public void CreateGUI()
        {
            var settings = BehaviourTreeSettings.GetOrCreate();

            VisualElement root = rootVisualElement;

            var visualTree = settings.treeUxml;
            visualTree.CloneTree(root);

            var styleSheet = settings.treeUss;
            root.styleSheets.Add(styleSheet);

            treeView = root.Q<BehaviourTreeView>();
            treeView.nodeSelectedCallback = OnNodeSelected;
            treeView.nodeUnselectedCallback = OnNodeUnselected;
            treeView.undoChangedCallback = CheckUndoRedo;

            inspectorView = root.Q<InspectorView>();
            variables = root.Q<IMGUIContainer>("variables");

            assetsMenu = root.Q<ToolbarMenu>("assets");
            assetsMenu.menu.AppendAction("新建", (a) => CreateNewTree("new behaviour tree"));
            CheckAssetDropdown();
            exeMenu = root.Q<ToolbarMenu>("exe-select");
            CheckExeDropdown();

            undo = root.Q<ToolbarButton>("undo");
            undo.clicked += OnUndoClick;
            redo = root.Q<ToolbarButton>("redo");
            redo.clicked += OnRedoClick;
            CheckUndoRedo();

            treeName = root.Q<Label>("tree-name");

            shared = root.Q<Button>("shared");
            shared.clicked += OnSharedClick;
            global = root.Q<Button>("global");
            global.clicked += OnGlobalClick;
            showShared = true;
            serializedGlobal = new SerializedObject(Application.isPlaying && BehaviourManager.Instance ? BehaviourManager.Instance.GlobalVariables : ZetanEditorUtility.LoadAsset<GlobalVariables>());
            CheckShowShared();

            if (tree == null) OnSelectionChange();
            else SelectTree(tree);
            bool goTree = false;
            if (Selection.activeGameObject)
            {
                BehaviourExecutor exe = Selection.activeGameObject.GetComponent<BehaviourExecutor>();
                if (exe)
                {
                    goTree = true;
                    treeName.text = $"行为树视图\t当前路径：{exe.gameObject.GetPath()} <{exe.GetType().Name}.{ZetanUtility.GetMemberName(() => exe.Behaviour)}>";
                }
            }
            if (tree && !goTree) treeName.text = $"行为树视图\t当前路径：{AssetDatabase.GetAssetPath(tree)}";
        }
        private void OnSelectionChange()
        {
            EditorApplication.delayCall += () =>
            {
                bool goTree = false;
                if (treeName != null) treeName.text = "行为树视图";
                BehaviourTree tree = Selection.activeObject as BehaviourTree;
                if (!tree)
                {
                    if (Selection.activeGameObject)
                    {
                        BehaviourExecutor exe = Selection.activeGameObject.GetComponent<BehaviourExecutor>();
                        if (exe)
                        {
                            tree = exe.Behaviour;
                            goTree = true;
                            if (treeName != null) treeName.text = $"行为树视图\t当前路径：{exe.gameObject.GetPath()} <{exe.GetType().Name}.{ZetanUtility.GetMemberName(() => exe.Behaviour)}>";
                        }
                    }
                }
                SelectTree(tree);
                if (this.tree && !goTree && treeName != null) treeName.text = $"行为树视图\t当前路径：{AssetDatabase.GetAssetPath(this.tree)}";
            };
        }
        private void OnInspectorUpdate()
        {
            treeView?.OnUpdate();
            if (!treeView?.tree) treeView?.DrawTreeView(tree);
        }
        private void OnProjectChange()
        {
            CheckAssetDropdown();
        }
        private void OnHierarchyChange()
        {
            CheckAssetDropdown();
        }
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            OnSelectionChange();
        }
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
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
            if (!showShared && (serializedObject == null || serializedObject.targetObject == null))
            {
                if (GUILayout.Button("新建")) CreateGlobalVariables("global variables");
            }
            if (serializedObject == null || serializedVariables == null) return;
            if (serializedObject.targetObject)
            {
                serializedObject.Update();
                variableList?.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion

        #region 新建相关
        private void CreateNewTree(string assetName)
        {
            BehaviourTree tree = ZetanEditorUtility.SaveFilePanel(CreateInstance<BehaviourTree>, assetName, true, true);
            if (tree)
            {
                serializedGlobal = new SerializedObject(tree);
                CheckShowShared();
            }
        }
        private void CreateGlobalVariables(string assetName)
        {
            if (serializedGlobal != null && serializedGlobal.targetObject != null) return;
            GlobalVariables global = ZetanEditorUtility.SaveFilePanel(CreateInstance<GlobalVariables>, assetName);
            if (global)
            {
                serializedGlobal = new SerializedObject(global);
                CheckShowShared();
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
                BehaviourTree localTree = ZetanEditorUtility.SaveFilePanel(() => BehaviourTree.ConvertToLocal(tree), assetName, true);
                for (int i = 0; i < localTree.Nodes.Count; i++)
                {
                    AssetDatabase.AddObjectToAsset(localTree.Nodes[i], localTree);
                }
                AssetDatabase.SaveAssets();
            }
        }
        #endregion
    }
}