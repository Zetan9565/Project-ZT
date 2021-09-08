using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using ZetanExtends;

namespace ZetanStudio.BehaviourTree
{
    public class BehaviourTreeEditor : EditorWindow
    {
        #region 视图相关
        private BehaviourTreeView treeView;
        private BehaviourTree tree;
        private InspectorView inspectorView;
        private IMGUIContainer variables;
        private ToolbarMenu toolbarMenu;
        private ToolbarButton undo;
        private ToolbarButton redo;
        private ToolbarSearchField searchField;
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
        private ReorderableList variableList;
        #endregion

        #region 静态回调
        [MenuItem("Zetan Studio/行为树编辑器")]
        public static void CreateWindow()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("行为树编辑器");
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

            if (treeView == null || !selected)
            {
                return;
            }

            tree = selected;

            treeView.DrawTreeView(tree);

            serializedTree = new SerializedObject(tree);
            var global = ZetanEditorUtility.LoadAssets<GlobalVariables>().Find(g => g);
            serializedGlobal = new SerializedObject(global);
            CheckShowShared();

            EditorApplication.delayCall += () =>
            {
                treeView.FrameAll();
            };
        }
        private void CheckAssetDropdown()
        {
            var behaviourTrees = ZetanEditorUtility.LoadAssets<BehaviourTree>();
            behaviourTrees.ForEach(tree =>
            {
                toolbarMenu.menu.AppendAction($"{ZetanEditorUtility.GetDirectoryName(tree).Replace("\\", "/").Replace("Assets/", "")}/{tree.name}", (a) =>
                {
                    Selection.activeObject = tree;
                });
            });
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
            inspectorView?.InspectNode(tree, selected);
        }
        private void OnNodeUnselected(NodeEditor unseleted)
        {
            if (inspectorView != null)
                if (inspectorView.nodeEditor == unseleted)
                    inspectorView.Clear();
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
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/BehaviourSystem/Editor/BehaviourTreeEditor.uxml");
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/BehaviourSystem/Editor/BehaviourTreeEditor.uss");
            root.styleSheets.Add(styleSheet);

            treeView = root.Q<BehaviourTreeView>();
            treeView.nodeSelectedCallback = OnNodeSelected;
            treeView.nodeUnselectedCallback = OnNodeUnselected;
            treeView.undoChangedCallback = CheckUndoRedo;

            inspectorView = root.Q<InspectorView>();
            variables = root.Q<IMGUIContainer>("variables");

            toolbarMenu = root.Q<ToolbarMenu>("assets");
            toolbarMenu.menu.AppendAction("新建", (a) => CreateNewTree("new behaviour tree"));
            toolbarMenu.menu.AppendSeparator();
            CheckAssetDropdown();

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
            CheckShowShared();

            if (tree == null) OnSelectionChange();
            else SelectTree(tree);
        }
        private void OnSelectionChange()
        {
            EditorApplication.delayCall += () =>
            {
                bool goTree = false;
                treeName.text = "行为树视图";
                BehaviourTree tree = Selection.activeObject as BehaviourTree;
                if (!tree)
                {
                    if (Selection.activeGameObject)
                    {
                        BehaviourExecutor exe = Selection.activeGameObject.GetComponent<BehaviourExecutor>();
                        if (exe)
                        {
                            tree = exe.behaviour;
                            goTree = true;
                            treeName.text = $"行为树视图\t当前：{exe.gameObject.GetPath()} ({exe.GetType().Name}.{ZetanUtility.GetMemberName(() => exe.behaviour)})";
                        }
                    }
                }
                SelectTree(tree);
                if (this.tree && !goTree) treeName.text = $"行为树视图\t当前：{AssetDatabase.GetAssetPath(this.tree)}";
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
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
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
            if (serializedObject == null || serializedVariables == null) return;
            variableList = new ReorderableList(serializedObject, serializedVariables, true, false, true, true);
            variableList.drawElementCallback = (rect, index, isFocused, isActive) =>
            {
                if (serializedObject.targetObject)
                {
                    serializedObject.Update();
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty variable = serializedVariables.GetArrayElementAtIndex(index);
                    int lineCount = 0;
                    //variable.NextVisible(true);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), variable, true);
                    lineCount++;
                    //if (variable.isExpanded)
                    //{
                    //    SerializedProperty end = variable.GetEndProperty();
                    //    while (variable.NextVisible(true) && !SerializedProperty.EqualContents(end, variable))
                    //    {
                    //        EditorGUI.PropertyField(new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2) * lineCount, rect.width, EditorGUIUtility.singleLineHeight), variable);
                    //        lineCount++;
                    //    }
                    //}
                    if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                }
            };
            variableList.elementHeightCallback = (index) =>
            {
                SerializedProperty variable = serializedVariables.GetArrayElementAtIndex(index);
                int lineCount = 1;
                if (variable.isExpanded)
                {
                    SerializedProperty end = variable.GetEndProperty();
                    while (variable.NextVisible(true) && !SerializedProperty.EqualContents(end, variable))
                    {
                        lineCount++;
                    }
                }
                return lineCount * (EditorGUIUtility.singleLineHeight + 2);
            };
            variableList.onAddDropdownCallback = (rect, list) =>
            {
                GenericMenu menu = new GenericMenu();
                foreach (var type in TypeCache.GetTypesDerivedFrom<SharedVariable>())
                {
                    if (!type.IsGenericType) menu.AddItem(new GUIContent(type.Name), false, () => { InserNewVariable(type); });
                }
                menu.DropDown(rect);
            };
        }
        private void InserNewVariable(Type type)
        {
            serializedObject.Update();
            int index = serializedVariables.arraySize;
            serializedVariables.InsertArrayElementAtIndex(index);
            SerializedProperty element = serializedVariables.GetArrayElementAtIndex(index);
            SharedVariable variable = (SharedVariable)Activator.CreateInstance(type);
            variable.isShared = showShared;
            variable.isGlobal = !showShared;
            element.managedReferenceValue = variable;
            serializedObject.ApplyModifiedProperties();
        }
        private void DrawVariables()
        {
            if (!showShared && (serializedObject == null || serializedObject.targetObject == null))
            {
                if (GUILayout.Button("新建"))
                {
                    CreateGlobalVariables("global variables");
                }
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

        #region 新建
        private void CreateNewTree(string assetName)
        {
        selection:
            string path = EditorUtility.SaveFilePanel("新建行为树", string.Empty, assetName, "asset");
            if (!string.IsNullOrEmpty(path))
            {
                if (ZetanEditorUtility.IsValidPath(path))
                {
                    try
                    {
                        BehaviourTree objectInstance = CreateInstance<BehaviourTree>();
                        AssetDatabase.CreateAsset(objectInstance, AssetDatabase.GenerateUniqueAssetPath(path.Replace(Application.dataPath, "Assets")));
                        AssetDatabase.Refresh();
                        Selection.activeObject = objectInstance;
                        EditorGUIUtility.PingObject(objectInstance);
                    }
                    catch
                    {
                        if (EditorUtility.DisplayDialog("新建失败", "请选择Assets目录或以下的文件夹。", "确定", "取消"))
                            goto selection;
                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog("提示", "请选择Assets目录或以下的文件夹。", "确定", "取消"))
                        goto selection;
                }
            }
        }
        private void CreateGlobalVariables(string assetName)
        {
            if (serializedGlobal != null && serializedGlobal.targetObject != null) return;
            selection:
            string path = EditorUtility.SaveFilePanel("新建全局变量", string.Empty, assetName, "asset");
            if (!string.IsNullOrEmpty(path))
            {
                if (ZetanEditorUtility.IsValidPath(path))
                {
                    try
                    {
                        GlobalVariables objectInstance = CreateInstance<GlobalVariables>();
                        AssetDatabase.CreateAsset(objectInstance, AssetDatabase.GenerateUniqueAssetPath(path.Replace(Application.dataPath, "Assets")));
                        AssetDatabase.Refresh();
                        serializedGlobal = new SerializedObject(objectInstance);
                        CheckShowShared();
                    }
                    catch
                    {
                        if (EditorUtility.DisplayDialog("新建失败", "请选择Assets目录或以下的文件夹。", "确定", "取消"))
                            goto selection;
                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog("提示", "请选择Assets目录或以下的文件夹。", "确定", "取消"))
                        goto selection;
                }
            }
        }
        #endregion
    }
}