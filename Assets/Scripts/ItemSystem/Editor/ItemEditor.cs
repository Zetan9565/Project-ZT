using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZetanStudio.Item.Module;

namespace ZetanStudio.Item.Editor
{
    public sealed class ItemEditor : EditorWindow
    {
        private ItemEditorSettings settings;

        private ToolbarSearchField searchField;
        private DropdownField templateSelector;
        private Label listLabel;
        private UnityEngine.UIElements.ListView itemList;
        private UnityEngine.UIElements.ListView templateList;
        private VisualElement itemContainer;
        private VisualElement templateContainer;
        private ScrollView rightPanel;
        private TabbedBar funcTab;
        private ItemNew selectedItem;
        private IEnumerable<ItemNew> selectedItems;
        private SerializedObject serializedItem;
        private ItemTemplate selectedTemplate;
        private IEnumerable<ItemTemplate> selectedTemplates;
        private SerializedObject serializedTemplate;
        private ItemTemplate currentTemplate;
        private List<ItemNew> items;
        private List<ItemTemplate> templates;
        private List<string> templateNames;
        private Button deleteButton;
        private ObjectField oldItem;
        private UnityEngine.UIElements.ListView searchDropdown;
        private DropdownField searchSelector;
        private SearchKeyType keyType;

        private bool useDatabase;

        private enum SearchKeyType
        {
            SearchName,
            SearchID,
            SearchDescription,
        }

        #region 静态方法
        [MenuItem("Zetan Studio/道具编辑器")]
        public static void CreateWindow()
        {
            ItemEditor wnd = GetWindow<ItemEditor>();
            wnd.minSize = ItemEditorSettings.GetOrCreate().minWindowSize;
            wnd.titleContent = new GUIContent("道具编辑器");
        }
        public static void CreateWindow(ItemNew item)
        {
            ItemEditor wnd = GetWindow<ItemEditor>();
            wnd.minSize = ItemEditorSettings.GetOrCreate().minWindowSize;
            wnd.titleContent = new GUIContent("道具编辑器");
            EditorApplication.delayCall += () => wnd.itemList.SetSelection(wnd.items.IndexOf(item));
        }
        public static void CreateWindow(ItemTemplate template)
        {
            ItemEditor wnd = GetWindow<ItemEditor>();
            wnd.minSize = ItemEditorSettings.GetOrCreate().minWindowSize;
            wnd.titleContent = new GUIContent("道具编辑器");
            EditorApplication.delayCall += () =>
            {
                wnd.funcTab.SetSelected(2);
                wnd.StartCoroutine(selete());

                IEnumerator selete()
                {
                    yield return new WaitForEndOfFrame();
                    wnd.templateList.SetSelection(wnd.templates.IndexOf(template));
                }
            };
        }
        [OnOpenAsset]
#pragma warning disable IDE0060 // 删除未使用的参数
        public static bool OnOpenAsset(int instanceID, int line)
#pragma warning restore IDE0060 // 删除未使用的参数
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is ItemNew item)
            {
                CreateWindow(item);
                return true;
            }
            return false;
        }
        #endregion

        #region Unity回调
        public void CreateGUI()
        {
            try
            {
                settings = settings ? settings : ItemEditorSettings.GetOrCreate();
                useDatabase = settings.useDatabase;

                VisualElement root = rootVisualElement;

                var visualTree = settings.treeUxml;
                visualTree.CloneTree(root);
                var styleSheet = settings.treeUss;
                root.styleSheets.Add(styleSheet);

                searchField = root.Q<ToolbarSearchField>("search-input");
                searchField.RegisterValueChangedCallback(new EventCallback<ChangeEvent<string>>(evt =>
                {
                    DoSearchDropdown(evt.newValue);
                }));
                searchDropdown = root.Q<UnityEngine.UIElements.ListView>("search-dropdown");
                searchDropdown.makeItem = () => new Label() { enableRichText = true };
                searchDropdown.onSelectionChange += OnSearchListSelected;
                root.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (!string.IsNullOrEmpty(searchField.value) && !searchDropdown.Contains(evt.target as VisualElement))
                        searchField.value = string.Empty;
                });
                DoSearchDropdown();
                searchSelector = root.Q<DropdownField>("search-selector");
                searchSelector.RegisterValueChangedCallback(evt =>
                {
                    keyType = (SearchKeyType)searchSelector.choices.IndexOf(evt.newValue);
                });

                funcTab = root.Q<TabbedBar>();
                funcTab.Refresh(new string[] { "道具", "模板" }, OnFuncTab);
                funcTab.onRightClick = OnRightFuncTab;

                Button refresh = root.Q<Button>("refresh-button");
                refresh.clicked += Refresh;
                Button newButton = root.Q<Button>("new-button");
                newButton.clicked += OnNewClick;
                deleteButton = root.Q<Button>("delete-button");
                deleteButton.clicked += OnDeleteClick;
                RefreshDeleteButton();

                oldItem = root.Q<ObjectField>("old-item");
                oldItem.objectType = typeof(ItemBase);
                Button oldButton = root.Q<ToolbarButton>("copy-button");
                oldButton.clicked += OnCopyClick;
                oldButton = root.Q<ToolbarButton>("copy-all-button");
                oldButton.clicked += OnCopyAllClick;

                listLabel = root.Q<Label>("list-label");

                templateSelector = root.Q<DropdownField>("template-dropdown");
                templateSelector.RegisterValueChangedCallback(OnTemplateSelected);
                RefreshTemplateSelector();

                itemList = root.Q<UnityEngine.UIElements.ListView>("item-list");
                itemList.selectionType = SelectionType.Multiple;
                itemList.makeItem = () =>
                {
                    var label = new Label();
                    label.AddManipulator(new ContextualMenuManipulator(evt =>
                    {
                        if (label.userData is ItemNew item)
                        {
                            evt.menu.AppendAction("定位", a => EditorGUIUtility.PingObject(item));
                            evt.menu.AppendAction("删除", a => DeleteItem(item));
                        }
                    }));
                    return label;
                };
                itemList.bindItem = (e, i) =>
                {
                    (e as Label).text = !string.IsNullOrEmpty(items[i].Name) ? items[i].Name : "(未命名道具)";
                    e.userData = items[i];
                };
                itemList.onSelectionChange += (os) => OnListItemSelected(os.Select(x => x as ItemNew));
                RefreshItems();

                templateList = root.Q<UnityEngine.UIElements.ListView>("template-list");
                templateList.selectionType = SelectionType.Multiple;
                templateList.makeItem = () =>
                {
                    var label = new Label();
                    label.AddManipulator(new ContextualMenuManipulator(evt =>
                    {
                        if (label.userData is ItemTemplate template)
                        {
                            evt.menu.AppendAction("定位", a => EditorGUIUtility.PingObject(template));
                            evt.menu.AppendAction("删除", a => DeleteTemplate(template));
                        }
                    }));
                    return label;
                };
                templateList.bindItem = (e, i) =>
                {
                    (e as Label).text = !string.IsNullOrEmpty(templates[i].Name) ? templates[i].Name : "(未命名模板)";
                    e.userData = templates[i];
                };
                templateList.onSelectionChange += (os) => OnListTemplateSelected(os.Select(x => x as ItemTemplate));
                RefreshTemplates();

                rightPanel = root.Q<ScrollView>("right-panel");
                itemContainer = root.Q("item-container");
                templateContainer = root.Q("template-container");

                Undo.undoRedoPerformed += RefreshModules;

                funcTab.SetSelected(1);

                root.RegisterCallback(new EventCallback<KeyDownEvent>(evt =>
                {
                    if (hasFocus && evt.keyCode == KeyCode.Delete) OnDeleteClick();
                }));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        private void OnFocus()
        {
            if (settings && useDatabase != settings.useDatabase)
            {
                useDatabase = settings.useDatabase;
                if (funcTab != null)
                    switch (funcTab.SelectedIndex)
                    {
                        case 1:
                            itemList.ClearSelection();
                            RefreshItems();
                            break;
                    }
            }
        }
        private void OnProjectChange()
        {
            if (funcTab != null)
                switch (funcTab.SelectedIndex)
                {
                    case 1:
                        if (items.Exists(x => !x))
                        {
                            itemList.ClearSelection();
                            RefreshItems();
                        }
                        break;
                    case 2:
                        if (templates.Exists(x => !x))
                        {
                            templateList.ClearSelection();
                            RefreshTemplates();
                        }
                        break;
                }
        }
        private void OnDestroy()
        {
            Undo.undoRedoPerformed -= RefreshModules;
            if (selectedItem) Undo.ClearUndo(selectedItem);
            if (selectedTemplate) Undo.ClearUndo(selectedTemplate);
            AssetDatabase.SaveAssets();
        }
        #endregion

        #region 搜索相关
        private void DoSearchDropdown(string keyword = null)
        {
            searchDropdown.style.display = new StyleEnum<DisplayStyle>(string.IsNullOrEmpty(keyword) ? DisplayStyle.None : DisplayStyle.Flex);
            if (!string.IsNullOrEmpty(keyword))
            {
                switch (funcTab.SelectedIndex)
                {
                    case 1:
                        switch (keyType)
                        {
                            case SearchKeyType.SearchID:
                                var results = items.FindAll(x => x.ID.Contains(keyword));
                                searchDropdown.itemsSource = results;
                                searchDropdown.bindItem = (e, i) =>
                                {
                                    (e as Label).text = $"{results[i].Name}\n(ID: {ZetanUtility.Editor.HighlightContentByKey(results[i].ID, keyword, results[i].ID.Length)})";
                                };
                                searchDropdown.RefreshItems();
                                break;
                            case SearchKeyType.SearchName:
                                results = items.FindAll(x => x.Name.Contains(keyword));
                                searchDropdown.itemsSource = results;
                                searchDropdown.bindItem = (e, i) =>
                                {
                                    (e as Label).text = ZetanUtility.Editor.HighlightContentByKey(results[i].Name, keyword, results[i].Name.Length);
                                };
                                searchDropdown.RefreshItems();
                                break;
                            case SearchKeyType.SearchDescription:
                                results = items.FindAll(x => x.Description.Contains(keyword));
                                searchDropdown.itemsSource = results;
                                searchDropdown.bindItem = (e, i) =>
                                {
                                    (e as Label).text = $"{results[i].Name}\n(描述: {ZetanUtility.Editor.HighlightContentByKey(results[i].Description, keyword, 30)})";
                                };
                                searchDropdown.RefreshItems();
                                break;
                        }
                        break;
                    case 2:
                        switch (keyType)
                        {
                            case SearchKeyType.SearchID:
                                var results = templates.FindAll(x => x.IDPrefix.Contains(keyword));
                                searchDropdown.itemsSource = results;
                                searchDropdown.bindItem = (e, i) =>
                                {
                                    (e as Label).text = $"{results[i].Name}\n(ID前缀: {ZetanUtility.Editor.HighlightContentByKey(results[i].IDPrefix, keyword, results[i].IDPrefix.Length)})";
                                };
                                searchDropdown.RefreshItems();
                                break;
                            case SearchKeyType.SearchName:
                                results = templates.FindAll(x => x.Name.Contains(keyword));
                                searchDropdown.itemsSource = results;
                                searchDropdown.bindItem = (e, i) =>
                                {
                                    (e as Label).text = ZetanUtility.Editor.HighlightContentByKey(results[i].Name, keyword, results[i].Name.Length);
                                };
                                searchDropdown.RefreshItems();
                                break;
                        }
                        break;
                }
            }
        }
        private void OnSearchListSelected(IEnumerable<object> os)
        {
            if (os.FirstOrDefault() is ItemNew item)
                SelectListItem(items.IndexOf(item));
            else if (os.FirstOrDefault() is ItemTemplate template)
                SelectListTemplate(templates.IndexOf(template));
            searchDropdown.SetSelectionWithoutNotify(null);
            searchField.value = null;
        }
        #endregion

        #region 旧版导入相关
        private void OnCopyClick()
        {
            if (funcTab.SelectedIndex == 1 && selectedItem)
            {
                ItemNew.Editor.CopyFromOld(selectedItem, oldItem.value as ItemBase);
                oldItem.value = null;
                RefreshModules();
            }
        }
        private void OnCopyAllClick()
        {
            foreach (var old in Resources.LoadAll<ItemBase>(""))
            {
                ItemNew item = CreateInstance<ItemNew>();
                ItemNew.Editor.CopyFromOld(item, old);
                AssetDatabase.CreateAsset(item, AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/Configuration/Item/{ObjectNames.NicifyVariableName(old.GetType().Name)}.asset"));
            }
            AssetDatabase.SaveAssets();
            itemList.ClearSelection();
            RefreshItems();
        }
        #endregion

        #region 删除相关
        private void OnDeleteClick()
        {
            switch (funcTab.SelectedIndex)
            {
                case 1:
                    if (selectedItems != null)
                    {
                        if (!settings.useDatabase)
                        {
                            if (EditorUtility.DisplayDialog("删除选中道具", $"确定将选中道具放入回收站吗？", "确定", "取消"))
                            {
                                List<string> failedPaths = new List<string>();
                                AssetDatabase.MoveAssetsToTrash(selectedItems.Select(x => AssetDatabase.GetAssetPath(x)).ToArray(), failedPaths);
                            }
                        }
                        else if (EditorUtility.DisplayDialog("删除选中道具", $"确定将选中道具从数据库中删除吗？此操作将不可逆！", "确定", "取消"))
                        {
                            bool success = false;
                            foreach (var item in selectedItems)
                            {
                                if (ItemDatabase.Editor.DeleteItem(item))
                                {
                                    items.Remove(item);
                                    success = true;
                                }
                            }
                            if (success)
                            {
                                itemList.ClearSelection();
                                itemList.RefreshItems();
                                rightPanel.Clear();
                            }
                        }
                    }
                    break;
                case 2:
                    if (selectedTemplates != null)
                    {
                        if (EditorUtility.DisplayDialog("删除选中模板", $"确定将选中模板放入回收站吗？", "确定", "取消"))
                        {
                            bool success = false;
                            foreach (var template in selectedTemplates)
                            {
                                if (AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(template)))
                                {
                                    templates.Remove(template);
                                    success = true;
                                }
                            }
                            if (success)
                            {
                                templateList.ClearSelection();
                                templateList.RefreshItems();
                                rightPanel.Clear();
                            }
                        }
                    }
                    break;
            }
        }
        private void RefreshDeleteButton()
        {
            switch (funcTab.SelectedIndex)
            {
                case 1: deleteButton.SetEnabled(selectedItems != null && selectedItems.Count() > 0); break;
                case 2: deleteButton.SetEnabled(selectedTemplates != null && selectedTemplates.Count() > 0); break;
                default: deleteButton.SetEnabled(false); break;
            }
        }
        private void DeleteItem(ItemNew item)
        {
            if (!settings.useDatabase)
            {
                if (EditorUtility.DisplayDialog("删除选中道具", $"确定将道具 [{item.Name}(ID: {item.ID})] 放入回收站吗？", "确定", "取消"))
                {
                    if (!AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(item))) return;
                }
                else return;
            }
            else if (EditorUtility.DisplayDialog("删除选中道具", $"确定将道具 [{item.Name}(ID: {item.ID})] 从数据库中删除吗？此操作将不可逆！", "确定", "取消"))
            {
                if (ItemDatabase.Editor.DeleteItem(item))
                {
                    items.Remove(item);
                    itemList.ClearSelection();
                    itemList.RefreshItems();
                    rightPanel.Clear();
                }
            }
        }
        private void DeleteTemplate(ItemTemplate template)
        {
            if (EditorUtility.DisplayDialog("删除选中模板", $"确定将模板 [{template.Name}] 放入回收站吗？", "确定", "取消"))
            {
                if (AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(template)))
                {
                    templates.Remove(template);
                    templateList.ClearSelection();
                    templateList.RefreshItems();
                    rightPanel.Clear();
                }
            }
        }
        #endregion

        #region 道具相关
        private void SelectListItem(int index)
        {
            itemList.SetSelection(index);
            itemList.ScrollToItem(index);
        }
        private void OnListItemSelected(IEnumerable<ItemNew> items)
        {
            rightPanel.Clear();
            selectedItems = items;
            if (items != null && items.Count() == 1)
            {
                var item = items.FirstOrDefault();
                selectedItem = item;
                serializedItem = item ? new SerializedObject(item) : null;
                RefreshDeleteButton();
                if (!item) return;
                templateList.ClearSelection();
                ItemBaseInfoBlock baseInfo = new ItemBaseInfoBlock(new SerializedObject(item), currentTemplate ? currentTemplate.IDPrefix : null)
                {
                    onInspectorChanged = () => itemList.RefreshItem(this.items.IndexOf(item))
                };
                rightPanel.Add(baseInfo);
                MakeAddModuleButton(item.Modules);
                RefreshModules();
            }
            else
            {
                selectedItem = null;
                serializedItem = null;
            }
        }
        private void RefreshItems()
        {
            if (!settings.useDatabase)
                items = ZetanUtility.Editor.LoadAssets<ItemNew>().FindAll(x => !AssetDatabase.IsSubAsset(x) && ItemNew.Editor.MatchTemplate(x, currentTemplate));
            else
            {
                if (!currentTemplate) items = ItemDatabase.Editor.GetItems();
                else items = ItemDatabase.Editor.GetItems().FindAll(x => ItemNew.Editor.MatchTemplate(x, currentTemplate));
            }
            items.Sort(ItemNew.ItemComparer.Default.Compare);
            if (itemList != null)
            {
                itemList.itemsSource = items;
                itemList.RefreshItems();
            }
            RefreshDeleteButton();
        }
        private void OnTemplateSelected(ChangeEvent<string> s)
        {
            currentTemplate = templates?.Find(x => x.Name == s.newValue);
            RefreshItems();
        }
        #endregion

        #region 脚本相关
        private void EditScript(ModuleBlock block)
        {
            if (block != null)
            {
                var script = ZetanUtility.Editor.LoadAssets<MonoScript>().Find(x => x.GetClass() == block.GetType());
                if (script) AssetDatabase.OpenAsset(script);
            }
        }
        private void EditScript(ItemModule module)
        {
            if (module != null)
            {
                var script = ZetanUtility.Editor.LoadAssets<MonoScript>().Find(x => x.GetClass() == module.GetType());
                if (script) AssetDatabase.OpenAsset(script);
            }
        }
        #endregion

        #region 模块相关
        private void OnSelectType(Type type)
        {
            if (!typeof(ItemModule).IsAssignableFrom(type) || !selectedItem && !selectedTemplate) return;
            SerializedObject serializedObject = null;
            IList<ItemModule> modules = null;
            string undoName = null;
            Func<ItemModule> addModule = null;
            ScriptableObject scriptableObject = null;
            switch (funcTab.SelectedIndex)
            {
                case 1:
                    serializedObject = serializedItem;
                    modules = selectedItem.Modules;
                    undoName = $"添加模块至道具{selectedItem.Name}";
                    addModule = () => ItemNew.Editor.AddModule(selectedItem, type);
                    scriptableObject = selectedItem;
                    break;
                case 2:
                    serializedObject = serializedTemplate;
                    modules = selectedTemplate.Modules;
                    undoName = $"添加模块至模板{selectedTemplate.Name}";
                    addModule = () => ItemTemplate.Editor.AddModule(selectedTemplate, type);
                    scriptableObject = selectedTemplate;
                    break;
            }
            if (!CommonModule.IsCommon(type) && modules.Any(x => ItemModule.Duplicate(x, type)))
                EditorUtility.DisplayDialog("无法添加", $"已经存在 [{ItemModule.GetName(type)}] 模块，每种模块只能添加一个。", "确定");
            else if (serializedObject != null)
            {
                Undo.RegisterCompleteObjectUndo(scriptableObject, undoName);
                if (addModule() != null)
                {
                    RefreshModules();
                    this.StartCoroutine(scollToEnd());

                    IEnumerator scollToEnd()
                    {
                        yield return new WaitForEndOfFrame();
                        rightPanel.verticalScroller.value = rightPanel.verticalScroller.highValue;
                    }
                }
            }
        }
        private void MakeAddModuleButton(IEnumerable<ItemModule> modules)
        {
            VisualElement space = new VisualElement();
            space.style.height = 7;
            rightPanel.Add(space);
            var types = TypeCache.GetTypesDerivedFrom<ItemModule>().Where(x => !x.IsAbstract && (typeof(CommonModule).IsAssignableFrom(x) || !modules.Any(y => y.GetType() == x)));
            Button button = new Button() { text = "添加模块" };
            button.clicked += () =>
            {
                //var dropdown = new AdvancedDropdown<Type>("可用模块", types, OnSelectType, ItemModule.GetName, iconGetter: t => EditorGUIUtility.FindTexture("cs Script Icon"));
                //dropdown.Show(button.layout);
                TypeSearchProvider.OpenWindow<ItemModule>(new UnityEditor.Experimental.GraphView.SearchWindowContext(Event.current.mousePosition), OnSelectType, types, "可用模块", ItemModule.GetName, t => null, newScriptMaker: getScriptTemplate);

                void getScriptTemplate(out string fileName, out string path, out TextAsset template)
                {
                    fileName = "NewModule.cs";
                    path = settings.newScriptFolder;
                    template = settings.scriptTemplate;
                }
            };
            button.style.width = 230;
            button.style.height = 25;
            button.style.alignSelf = new StyleEnum<Align>(Align.Center);
            rightPanel.Add(button);
        }
        private void RefreshModules()
        {
            if (!selectedItem && !selectedTemplate) return;
            for (int i = 0; i < rightPanel.childCount; i++)
            {
                if (rightPanel[i] is ModuleBlock)
                {
                    rightPanel.RemoveAt(i);
                    i--;
                }
            }
            SerializedObject serializedObject = null;
            IList<ItemModule> modules = null;
            switch (funcTab.SelectedIndex)
            {
                case 1:
                    serializedObject = serializedItem;
                    modules = selectedItem.Modules;
                    break;
                case 2:
                    serializedObject = serializedTemplate;
                    modules = selectedTemplate.Modules;
                    break;
            }
            if (serializedObject != null && modules != null)
            {
                serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty modulesProp = serializedObject.FindProperty("modules");
                for (int i = 0; i < modulesProp.arraySize; i++)
                {
                    SerializedProperty property = modulesProp.GetArrayElementAtIndex(i);
                    rightPanel.Insert(rightPanel.childCount - 2, MakeModuleBlock(property, modules[i]));
                }
            }
        }
        private ModuleBlock MakeModuleBlock(SerializedProperty property, ItemModule module)
        {
            ModuleBlock block = ModuleBlock.Create(property, module);
            block.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                IList<ItemModule> Modules = null;
                switch (funcTab.SelectedIndex)
                {
                    case 1: if (selectedItem) Modules = selectedItem.Modules; break;
                    case 2: if (selectedTemplate) Modules = selectedTemplate.Modules; break;
                }
                if (Modules != null)
                {
                    int index = Modules.IndexOf(module);
                    evt.menu.AppendAction("移除", a => DeleteModule(module));
                    if (index > 0) evt.menu.AppendAction("上移", a => MoveModuleUp(module));
                    if (index < Modules.Count - 1) evt.menu.AppendAction("下移", a => MoveModuleDown(module));
                    evt.menu.AppendSeparator();
                    evt.menu.AppendAction("编辑脚本", a => EditScript(module));
                    evt.menu.AppendAction("编辑Editor脚本", a => EditScript(block));
                }
            }));
            return block;
        }
        private void MoveModuleUp(ItemModule module)
        {
            int index = -1;
            SerializedObject serializedObject = null;
            switch (funcTab.SelectedIndex)
            {
                case 1:
                    index = selectedItem.Modules.IndexOf(module);
                    serializedObject = serializedItem;
                    break;
                case 2:
                    index = selectedTemplate.Modules.IndexOf(module);
                    serializedObject = serializedTemplate;
                    break;
            }
            if (serializedObject != null && index > 0)
            {
                SerializedProperty modulesProp = serializedObject.FindProperty("modules");
                modulesProp.MoveArrayElement(index, index - 1);
                serializedObject.ApplyModifiedProperties();
                RefreshModules();
            }
        }
        private void MoveModuleDown(ItemModule module)
        {
            int index = -1, count = 0;
            SerializedObject serializedObject = null;
            switch (funcTab.SelectedIndex)
            {
                case 1:
                    index = selectedItem.Modules.IndexOf(module);
                    serializedObject = serializedItem;
                    count = selectedItem.Modules.Count;
                    break;
                case 2:
                    index = selectedTemplate.Modules.IndexOf(module);
                    serializedObject = serializedTemplate;
                    count = selectedTemplate.Modules.Count;
                    break;
            }
            if (serializedObject != null && index >= 0 && index < count - 1)
            {
                SerializedProperty modulesProp = serializedObject.FindProperty("modules");
                modulesProp.MoveArrayElement(index, index + 1);
                serializedObject.ApplyModifiedProperties();
                RefreshModules();
            }
        }
        private void DeleteModule(ItemModule module)
        {
            IList<ItemModule> modules = null;
            switch (funcTab.SelectedIndex)
            {
                case 1:
                    modules = selectedItem.Modules;
                    break;
                case 2:
                    modules = selectedTemplate.Modules;
                    break;
            }
            foreach (var exist in modules)
            {
                Type type = exist.GetType();
                var attr = type.GetCustomAttribute<ItemModule.RequireAttribute>();
                if (attr != null && attr.modules.Contains(module.GetType()))
                {
                    EditorUtility.DisplayDialog("无法移除", $"因为 [{ItemModule.GetName(type)}] 模块依赖于此模块，暂时无法移除。", "确定");
                    return;
                }
            }
            switch (funcTab.SelectedIndex)
            {
                case 1:
                    Undo.RegisterCompleteObjectUndo(selectedItem, $"从道具{selectedItem.Name}删除模块");
                    if (ItemNew.Editor.RemoveModule(selectedItem, module)) RefreshModules();
                    break;
                case 2:
                    Undo.RegisterCompleteObjectUndo(selectedTemplate, $"从模板{selectedTemplate.Name}删除模块");
                    if (ItemTemplate.Editor.RemoveModule(selectedTemplate, module)) RefreshModules();
                    break;
            }
        }
        #endregion

        #region 模板相关
        private void SelectListTemplate(int index)
        {
            templateList.SetSelection(index);
            templateList.ScrollToItem(index);
        }
        private void OnListTemplateSelected(IEnumerable<ItemTemplate> templates)
        {
            rightPanel.Clear();
            selectedTemplates = templates;
            if (templates != null && templates.Count() == 1)
            {
                var template = templates.FirstOrDefault();
                selectedTemplate = template;
                serializedTemplate = template ? new SerializedObject(template) : null;
                RefreshDeleteButton();
                if (!template) return;
                itemList.ClearSelection();
                TemplateBaseInfoBlock baseInfo = new TemplateBaseInfoBlock(serializedTemplate)
                {
                    onInspectorChanged = () => templateList.RefreshItem(this.templates.IndexOf(template))
                };
                rightPanel.Add(baseInfo);
                MakeAddModuleButton(selectedTemplate.Modules);
                RefreshModules();
            }
            else
            {
                selectedTemplate = null;
                serializedTemplate = null;
            }
        }
        private void RefreshTemplates()
        {
            templates = ZetanUtility.Editor.LoadAssets<ItemTemplate>();
            if (templateList != null)
            {
                templateList.itemsSource = templates;
                templateList.RefreshItems();
            }
            RefreshDeleteButton();
        }
        private void RefreshTemplateSelector()
        {
            templates = ZetanUtility.Editor.LoadAssets<ItemTemplate>();
            templateNames = new List<string>() { "不指定模板" };
            templateNames.AddRange(templates.Select(x => x.Name));
            if (templateSelector != null)
            {
                templateSelector.choices = templateNames;
                templateSelector.value = templateNames[0];
            }
        }
        #endregion

        #region 功能页签相关
        public void OnFuncTab(int index)
        {
            switch (index)
            {
                case 1:
                    if (itemContainer != null) itemContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    if (templateContainer != null) templateContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    if (listLabel != null)
                    {
                        listLabel.text = "道具列表";
                        listLabel.style.backgroundColor = new StyleColor(new Color(0.33f, 0.43f, 0.34f, 1));
                    }
                    RefreshItems();
                    searchSelector.choices = new List<string>() { "名称", "ID", "描述" };
                    searchSelector.value = "名称";
                    break;
                case 2:
                    if (itemContainer != null) itemContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    if (templateContainer != null) templateContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    if (listLabel != null)
                    {
                        listLabel.text = "模板列表";
                        listLabel.style.backgroundColor = new StyleColor(new Color(0.33f, 0.40f, 0.43f, 1));
                    }
                    RefreshTemplates();
                    searchSelector.choices = new List<string>() { "名称", "前缀" };
                    searchSelector.value = "名称";
                    break;
            }
        }
        public void OnRightFuncTab(int index, ContextualMenuPopulateEvent evt)
        {
            if (index == funcTab.SelectedIndex)
                evt.menu.AppendAction("刷新", a =>
                {
                    Refresh();
                });
        }
        private void Refresh()
        {
            switch (funcTab.SelectedIndex)
            {
                case 1: RefreshItems(); break;
                case 2: RefreshTemplates(); break;
            }
        }
        #endregion

        #region 新建相关
        private void OnNewClick()
        {
            switch (funcTab.SelectedIndex)
            {
                case 1: NewItem(); break;
                case 2: NewTemplate(); break;
            }
        }
        private void NewItem()
        {
            ItemNew item;
            if (!settings.useDatabase)
            {
                item = ZetanUtility.Editor.SaveFilePanel(CreateInstance<ItemNew>);
                if (item)
                {
                    ItemNew.Editor.ApplyTemplate(item, currentTemplate);
                    ItemNew.Editor.SetAutoID(item, ZetanUtility.Editor.LoadAssets<ItemNew>(), currentTemplate ? currentTemplate.IDPrefix : null);
                    ZetanUtility.Editor.SaveChange(item);
                }
            }
            else item = ItemDatabase.Editor.MakeItem(currentTemplate);
            if (item)
            {
                items.Add(item);
                RefreshItems();

                this.StartCoroutine(scrollTo());

                IEnumerator scrollTo()
                {
                    yield return new WaitForEndOfFrame();
                    SelectListItem(items.IndexOf(item));
                }
            }
        }
        private void NewTemplate()
        {
            ItemTemplate template = ZetanUtility.Editor.SaveFilePanel(CreateInstance<ItemTemplate>);
            if (template)
            {
                templates.Add(template);
                RefreshItems();
                SelectListTemplate(templates.IndexOf(template));
            }
        }
        #endregion
    }
}