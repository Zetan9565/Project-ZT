using System;
using System.Collections.Generic;
using System.Linq;
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
                if (tree)
                    toolbarMenu?.menu.AppendAction($"{ZetanEditorUtility.GetDirectoryName(tree).Replace("\\", "/").Replace("Assets/", "")}/{tree.name}", (a) =>
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
            serializedGlobal = new SerializedObject(ZetanEditorUtility.LoadAsset<GlobalVariables>());
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
                    treeName.text = $"行为树视图\t当前：{exe.gameObject.GetPath()} ({exe.GetType().Name}.{ZetanUtility.GetMemberName(() => exe.Behaviour)})";
                }
            }
            if (tree && !goTree) treeName.text = $"行为树视图\t当前：{AssetDatabase.GetAssetPath(tree)}";
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
                            tree = exe.Behaviour;
                            goTree = true;
                            treeName.text = $"行为树视图\t当前：{exe.gameObject.GetPath()} ({exe.GetType().Name}.{ZetanUtility.GetMemberName(() => exe.Behaviour)})";
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
        selection:
            string path = EditorUtility.SaveFilePanel("新建行为树", string.Empty, assetName, "asset");
            if (!string.IsNullOrEmpty(path))
            {
                if (ZetanEditorUtility.IsValidPath(path))
                {
                    try
                    {
                        BehaviourTree objectInstance = CreateInstance<BehaviourTree>();
                        AssetDatabase.CreateAsset(objectInstance, AssetDatabase.GenerateUniqueAssetPath(ZetanEditorUtility.ConvertToAssetsPath(path)));
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
                        AssetDatabase.CreateAsset(objectInstance, AssetDatabase.GenerateUniqueAssetPath(ZetanEditorUtility.ConvertToAssetsPath(path)));
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

    public sealed class SharedVariableListDrawer
    {
        private readonly ReorderableList variableList;

        public SharedVariableListDrawer(SerializedObject serializedObject, SerializedProperty serializedVariables, bool isShared)
        {
            variableList = new ReorderableList(serializedObject, serializedVariables, true, true, true, true);
            variableList.drawElementCallback = (rect, index, isFocused, isActive) =>
            {
                if (serializedObject.targetObject)
                {
                    serializedObject.Update();
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty variable = serializedVariables.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), variable, true);
                    if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                }
            };
            variableList.elementHeightCallback = (index) =>
            {
                return EditorGUI.GetPropertyHeight(serializedVariables.GetArrayElementAtIndex(index), true);
            };
            variableList.onAddDropdownCallback = (rect, list) =>
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("新建类型"), false, CreateVariableScript);
                menu.AddSeparator("");
                foreach (var type in TypeCache.GetTypesDerivedFrom<SharedVariable>())
                {
                    if (!type.IsGenericType) menu.AddItem(new GUIContent(type.Name), false, () => { InsertNewVariable(type); });
                }
                menu.DropDown(rect);
            };
            variableList.onRemoveCallback = (list) =>
            {
                serializedObject.Update();
                SerializedProperty _name = serializedVariables.GetArrayElementAtIndex(list.index).FindPropertyRelative("_name");
                if (EditorUtility.DisplayDialog("删除变量", $"确定要删除变量 {_name.stringValue} 吗？", "确定", "取消"))
                    list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                serializedObject.ApplyModifiedProperties();
            };
            variableList.drawHeaderCallback = (rect) =>
            {
                string typeMsg = EditorUtility.IsPersistent(serializedObject.targetObject) ? "(不可引用场景对象)" : "(可引用场景对象)";
                EditorGUI.LabelField(rect, $"{(isShared ? "共享变量列表" : "全局变量列表")}{typeMsg}");
            };
            variableList.serializedProperty = serializedVariables;

            void InsertNewVariable(Type type)
            {
                serializedObject.Update();
                int index = serializedVariables.arraySize;
                serializedVariables.InsertArrayElementAtIndex(index);
                SerializedProperty element = serializedVariables.GetArrayElementAtIndex(index);
                SharedVariable variable = (SharedVariable)Activator.CreateInstance(type);
                string newName = $"{char.ToLower(type.Name[0])}{type.Name.Substring(1)}_{serializedVariables.arraySize}";
                variable.GetType().GetField("_name", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(variable, newName);
                variable.isShared = isShared;
                variable.isGlobal = !isShared;
                element.managedReferenceValue = variable;
                serializedObject.ApplyModifiedProperties();
                variableList.Select(serializedVariables.arraySize - 1);
            }
        }

        private void CreateVariableScript()
        {
            var settings = BehaviourTreeSettings.GetOrCreate();
            string path = $"{settings.newVarScriptFolder}/{ScriptTemplate.Variable.folder}";
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);

            UnityEngine.Object script = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            Selection.activeObject = script;
            EditorGUIUtility.PingObject(script);

            string templatePath = AssetDatabase.GetAssetPath(ScriptTemplate.Variable.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, ScriptTemplate.Variable.fileName);
        }

        public void DoLayoutList()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < variableList.serializedProperty.arraySize; i++)
            {
                SerializedProperty variable = variableList.serializedProperty.GetArrayElementAtIndex(i);
                string varName = variable.FindPropertyRelative("_name").stringValue;
                names.Add(varName);
            }
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    EditorGUILayout.HelpBox("有未命名的变量", MessageType.Error);
                    break;
                }
                else if (names.FindAll(x => x == name).Count > 1)
                {
                    EditorGUILayout.HelpBox("有名字重复的变量", MessageType.Error);
                    break;
                }
            }
            variableList.DoLayoutList();
        }
    }

    public sealed class SharedVariablePresetListDrawer
    {
        private readonly ReorderableList presetVariableList;

        private readonly string[] varType = { "填写", "选择" };

        public SharedVariablePresetListDrawer(SerializedObject serializedObject, SerializedProperty presetVariables, ISharedVariableHandler variableHandler, Func<int, Type> elementTypeGetter)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float lineHeightSpace = lineHeight + EditorGUIUtility.standardVerticalSpacing;
            presetVariableList = new ReorderableList(serializedObject, presetVariables, true, true, true, true);
            presetVariableList.drawElementCallback = (rect, index, isFocused, isActive) =>
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                SerializedProperty variable = presetVariables.GetArrayElementAtIndex(index);
                Type type = elementTypeGetter(index);
                SerializedProperty name = variable.FindPropertyRelative("_name");
                List<SharedVariable> variables = variableHandler.GetVariables(type);
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), variable,
                    new GUIContent(string.IsNullOrEmpty(name.stringValue) ? $"({type.Name})" : name.stringValue));
                Rect valueRect = new Rect(rect.x, rect.y + lineHeightSpace, rect.width * 0.84f, lineHeight);
                if (variable.isExpanded)
                {
                    SerializedProperty isShared = variable.FindPropertyRelative("isShared");
                    int typeIndex = isShared.boolValue ? 1 : 0;
                    if (typeIndex == 1)
                    {
                        string[] varNames = variables.Select(x => x.name).ToArray();
                        bool noNames = varNames.Length < 1;
                        int nameIndex = Array.IndexOf(varNames, name.stringValue);
                        if (noNames) varNames = varNames.Prepend("未选择").ToArray();
                        if (nameIndex < 0) nameIndex = 0;
                        nameIndex = EditorGUI.Popup(valueRect, "关联变量名称", nameIndex, varNames);
                        string nameStr = name.stringValue;
                        if (!noNames && nameIndex >= 0 && nameIndex < variables.Count) nameStr = varNames[nameIndex];
                        name.stringValue = nameStr;
                    }
                    else EditorGUI.PropertyField(valueRect, name, new GUIContent("关联变量名称"));
                    typeIndex = EditorGUI.Popup(new Rect(rect.x + rect.width * 0.84f + 2, rect.y + lineHeightSpace, rect.width * 0.16f - 2, lineHeight), typeIndex, varType);
                    switch (typeIndex)
                    {
                        case 0:
                            isShared.boolValue = false;
                            break;
                        case 1:
                            isShared.boolValue = true;
                            break;
                    }
                    SerializedProperty value = variable.FindPropertyRelative("value");
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, EditorGUI.GetPropertyHeight(value, true) - lineHeight), value, new GUIContent("预设值"), true);
                }
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            };
            presetVariableList.elementHeightCallback = (index) =>
            {
                SerializedProperty variable = presetVariables.GetArrayElementAtIndex(index);
                if (variable.isExpanded)
                {
                    return EditorGUI.GetPropertyHeight(presetVariables.GetArrayElementAtIndex(index), true);
                }
                else return lineHeightSpace;
            };
            presetVariableList.onAddDropdownCallback = (rect, list) =>
            {
                GenericMenu menu = new GenericMenu();
                foreach (var type in TypeCache.GetTypesDerivedFrom<SharedVariable>())
                {
                    if (!type.IsGenericType && type.BaseType.GetGenericArguments()[0].IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        menu.AddItem(new GUIContent($"自定义/{type.Name}"), false, () => { InsertNewVariable(type, false); });
                    }
                }
                List<SharedVariable> variables = ZetanEditorUtility.GetValue(presetVariables) as List<SharedVariable>;
                foreach (var variable in variableHandler.Variables)
                {
                    Type type = variable.GetType();
                    if (!variables.Exists(x => x.name == variable.name && x.GetType() == type) && type.BaseType.GetGenericArguments()[0].IsSubclassOf(typeof(UnityEngine.Object)))
                        menu.AddItem(new GUIContent(variable.name), false, () => { InsertNewVariable(type, true, variable.name); });
                }
                menu.DropDown(rect);
            };
            presetVariableList.onRemoveCallback = (list) =>
            {
                serializedObject.Update();
                SerializedProperty _name = presetVariables.GetArrayElementAtIndex(list.index).FindPropertyRelative("_name");
                if (EditorUtility.DisplayDialog("删除变量", $"确定要删除预设 {_name.stringValue} 吗？", "确定", "取消"))
                    list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                serializedObject.ApplyModifiedProperties();
            };
            presetVariableList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, $"变量预设列表");
            };
            presetVariableList.serializedProperty = presetVariables;

            void InsertNewVariable(Type type, bool select, string name = "")
            {
                serializedObject.Update();
                int index = presetVariables.arraySize;
                presetVariables.InsertArrayElementAtIndex(index);
                SerializedProperty element = presetVariables.GetArrayElementAtIndex(index);
                SharedVariable variable = (SharedVariable)Activator.CreateInstance(type);
                variable.GetType().GetField("_name", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(variable, name);
                variable.isShared = select;
                element.managedReferenceValue = variable;
                serializedObject.ApplyModifiedProperties();
                presetVariableList.Select(presetVariables.arraySize - 1);
            }
        }

        public void DoLayoutList()
        {
            presetVariableList.DoLayoutList();
        }
    }
}