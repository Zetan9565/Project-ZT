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

namespace ZetanStudio.ItemSystem.Editor
{
    using Module;
    using ZetanStudio.Editor;

    public sealed class ItemEditor : EditorWindow
    {
        #region 声明
        private ItemEditorSettings settings;

        private ToolbarSearchField searchField;
        private DropdownField templateSelector;
        private Label listLabel;
        private UnityEngine.UIElements.ListView itemList;
        private UnityEngine.UIElements.ListView templateList;
        private ScrollView rightPanel;
        private TabbedBar funcTab;
        private Item selectedItem;
        private IEnumerable<Item> selectedItems;
        private SerializedObject serializedItem;
        private ItemTemplate selectedTemplate;
        private IEnumerable<ItemTemplate> selectedTemplates;
        private SerializedObject serializedTemplate;
        private ItemTemplate currentTemplate;
        private List<Item> items;
        private List<ItemTemplate> templates;
        private List<string> templateNames;
        private Button deleteButton;
        private Button cloneButton;
        private UnityEngine.UIElements.ListView searchDropdown;
        private DropdownField searchSelector;
        private SearchKeyType keyType;
        private List<string> itemSearchType;
        private List<string> templateSearchType;
        [SerializeReference]
        private ItemModule copiedModule;

        private bool useDatabase;
        #endregion

        #region 静态方法
        [MenuItem("Window/Zetan Studio/道具编辑器")]
        public static void CreateWindow()
        {
            ItemEditor wnd = GetWindow<ItemEditor>();
            ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();
            wnd.minSize = settings.minWindowSize;
            wnd.titleContent = new GUIContent(L.Tr(settings.language, "道具编辑器"));
        }
        public static void CreateWindow(Item item)
        {
            ItemEditor wnd = GetWindow<ItemEditor>();
            ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();
            wnd.minSize = settings.minWindowSize;
            wnd.titleContent = new GUIContent(L.Tr(settings.language, "道具编辑器"));
            EditorApplication.delayCall += () => wnd.itemList.SetSelection(wnd.items.IndexOf(item));
        }
        public static void CreateWindow(ItemTemplate template)
        {
            ItemEditor wnd = GetWindow<ItemEditor>();
            ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();
            wnd.minSize = settings.minWindowSize;
            wnd.titleContent = new GUIContent(L.Tr(settings.language, "道具编辑器"));
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
        public static Item OpenAndCreateItem(ItemFilterAttribute itemFilter)
        {
            ItemEditor wnd = GetWindow<ItemEditor>();
            ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();
            wnd.minSize = settings.minWindowSize;
            wnd.titleContent = new GUIContent(L.Tr(settings.language, "道具编辑器"));
            if (itemFilter != null) wnd.NewItem(itemFilter);
            else wnd.NewItem();
            return wnd.selectedItem;
        }
        [OnOpenAsset]
#pragma warning disable IDE0060 // 删除未使用的参数
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is Item item)
            {
                CreateWindow(item);
                return true;
            }
            return false;
        }
#pragma warning restore IDE0060 // 删除未使用的参数

        [InitializeOnLoadMethod]
        private static void ClearInvalid()
        {
            //foreach (var item in Item.Editor.GetItems())
            //{
            //    if (Item.Editor.ClearInvalidModule(item))
            //        Debug.LogWarning(Localization.Tr(ItemEditorSettings.GetOrCreate().language, "道具 [{0}] 存在失效模块，已自动移除。", item.Name));
            //}
            //foreach (var template in ZetanUtility.Editor.LoadAssets<ItemTemplate>())
            //{
            //    if (ItemTemplate.Editor.ClearInvalidModule(template))
            //        Debug.LogWarning(Localization.Tr(ItemEditorSettings.GetOrCreate().language, "模板 [{0}] 存在失效模块，已自动移除。", template.Name));
            //}
        }
        #endregion

        #region Unity回调
        public void CreateGUI()
        {
            try
            {
                settings = settings ? settings : ItemEditorSettings.GetOrCreate();
                useDatabase = Item.UseDatabase;
                itemSearchType = new List<string>(L.TrM(settings.language, "全部", "名称", "ID", "类型", "描述", "模块"));
                templateSearchType = new List<string>(L.TrM(settings.language, "全部", "名称", "前缀", "类型", "描述", "模块"));

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
                funcTab.Refresh(new string[] { Tr("道具"), Tr("模板") }, OnFuncTab);
                funcTab.onRightClick = OnRightFuncTab;

                Button refresh = root.Q<Button>("refresh-button");
                refresh.clicked += Refresh;
                Button newButton = root.Q<Button>("new-button");
                newButton.clicked += OnNewClick;
                deleteButton = root.Q<Button>("delete-button");
                deleteButton.clicked += OnDeleteClick;
                RefreshDeleteButton();
                cloneButton = root.Q<Button>("clone-button");
                cloneButton.clicked += OnCloneClick;
                RefreshCloneButton();

                listLabel = root.Q<Label>("list-label");
                listLabel.AddToClassList("list-label");

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
                        if (label.userData is Item item)
                        {
                            evt.menu.AppendAction(Tr("定位"), a => EditorGUIUtility.PingObject(item));
                            evt.menu.AppendAction(Tr("删除"), a => DeleteItem(item));
                        }
                    }));
                    return label;
                };
                itemList.bindItem = (e, i) =>
                {
                    (e as Label).text = !string.IsNullOrEmpty(items[i].Name) ? items[i].Name : $"({(Tr("未命名道具"))})";
                    e.userData = items[i];
                };
                itemList.onSelectionChange += (os) => OnListItemSelected(os.Select(x => x as Item));
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
                            evt.menu.AppendAction(Tr("定位"), a => EditorGUIUtility.PingObject(template));
                            evt.menu.AppendAction(Tr("删除"), a => DeleteTemplate(template));
                        }
                    }));
                    return label;
                };
                templateList.bindItem = (e, i) =>
                {
                    (e as Label).text = !string.IsNullOrEmpty(templates[i].Name) ? templates[i].Name : $"({Tr("未命名模板")})";
                    e.userData = templates[i];
                };
                templateList.onSelectionChange += (os) => OnListTemplateSelected(os.Select(x => x as ItemTemplate));
                RefreshTemplates();

                rightPanel = root.Q<ScrollView>("right-panel");
                rightPanel.parent.RegisterCallback<DragEnterEvent>(evt =>
                {
                    if (funcTab.SelectedIndex == 1 && selectedItem || funcTab.SelectedIndex == 2 && selectedTemplate)
                        if (evt.target is not IMGUIContainer && DragAndDrop.objectReferences.All(x => x is MonoScript script && typeof(ItemModule).IsAssignableFrom(script.GetClass())))
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                });
                rightPanel.parent.RegisterCallback<DragUpdatedEvent>(evt =>
                {
                    if (funcTab.SelectedIndex == 1 && selectedItem || funcTab.SelectedIndex == 2 && selectedTemplate)
                        if (evt.target is not IMGUIContainer && DragAndDrop.objectReferences.All(x => x is MonoScript script && typeof(ItemModule).IsAssignableFrom(script.GetClass())))
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                });
                rightPanel.parent.RegisterCallback<DragPerformEvent>(evt =>
                {
                    if (funcTab.SelectedIndex == 1 && selectedItem || funcTab.SelectedIndex == 2 && selectedTemplate)
                        if (evt.target is not IMGUIContainer && DragAndDrop.objectReferences.All(x => x is MonoScript script && typeof(ItemModule).IsAssignableFrom(script.GetClass())))
                            foreach (var obj in DragAndDrop.objectReferences)
                            {
                                if (obj is MonoScript script)
                                {
                                    var target = evt.target as VisualElement;
                                    if (target.parent is not Toggle toggle || toggle.parent is not ModuleBlock block)
                                        AddModule(script.GetClass());
                                    else if (funcTab.SelectedIndex == 1 && selectedItem)
                                        AddModule(script.GetClass(), selectedItem.Modules.IndexOf(block.userData as ItemModule));
                                    else if (funcTab.SelectedIndex == 2 && selectedTemplate)
                                        AddModule(script.GetClass(), selectedTemplate.Modules.IndexOf(block.userData as ItemModule));
                                }
                            }
                });
                rightPanel.parent.RegisterCallback<DragLeaveEvent>(evt =>
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.None;
                });

                Undo.undoRedoPerformed += RefreshInspector;

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
            if (settings && useDatabase != Item.UseDatabase)
            {
                useDatabase = Item.UseDatabase;
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
            Undo.undoRedoPerformed -= RefreshInspector;
            if (selectedItem) Undo.ClearUndo(selectedItem);
            if (selectedTemplate) Undo.ClearUndo(selectedTemplate);
            AssetDatabase.SaveAssets();
        }
        #endregion

        #region 搜索相关
        private void DoSearchDropdown(string keyword = null)
        {
            IList itemsSource = new List<object>();
            Action<VisualElement, int> bindItem = (e, i) => { };
            bool empty = string.IsNullOrEmpty(keyword);
            searchDropdown.style.display = new StyleEnum<DisplayStyle>(empty ? DisplayStyle.None : DisplayStyle.Flex);
            if (!empty)
            {
                List<string> contents = new List<string>();
                switch (funcTab.SelectedIndex)
                {
                    case 1:
                        bool searchItemID(Item item, out string content)
                        {
                            content = null;
                            bool result = item.ID.Contains(keyword);
                            if (result) content = $"{item.Name}\n({Tr("ID")}: {ZetanUtility.Editor.HighlightContentByKey(item.ID, keyword, item.ID.Length)})";
                            return result;
                        }
                        bool searchItemName(Item item, out string content)
                        {
                            content = null;
                            bool result = item.Name.Contains(keyword);
                            if (result) content = $"{ZetanUtility.Editor.HighlightContentByKey(item.Name, keyword, item.Name.Length)}";
                            return result;
                        }
                        bool searchItemType(Item item, out string content)
                        {
                            content = null;
                            bool result = item.Type.Name.Contains(keyword);
                            if (result) content = $"{item.Name}\n({Tr("类型")}: {ZetanUtility.Editor.HighlightContentByKey(item.Type.Name, keyword, item.Type.Name.Length)})";
                            return result;
                        }
                        bool searchItemDesc(Item item, out string content)
                        {
                            content = null;
                            bool result = item.Description.Contains(keyword);
                            if (result) content = $"{item.Name}\n({Tr("描述")}: {ZetanUtility.Editor.HighlightContentByKey(item.Description, keyword, 30)})";
                            return result;
                        }
                        bool searchItemMod(Item item, out string content)
                        {
                            content = null;
                            bool result = item.Modules.Any(x => x.GetName().Contains(keyword));
                            if (result)
                            {
                                content = item.Modules.FirstOrDefault(x => x.GetName().Contains(keyword)).GetName();
                                content = $"{item.Name}\n({Tr("模块")}: {ZetanUtility.Editor.HighlightContentByKey(content, keyword, content.Length)})";
                            }
                            return result;
                        }

                        switch (keyType)
                        {
                            case SearchKeyType.SearchAll:
                                foreach (var item in items)
                                {
                                    if (searchItemID(item, out var content) || searchItemName(item, out content) || searchItemType(item, out content)
                                        || searchItemDesc(item, out content) || searchItemMod(item, out content))
                                    {
                                        itemsSource.Add(item);
                                        contents.Add(content);
                                    }
                                }
                                break;
                            case SearchKeyType.SearchID:
                                foreach (var item in items)
                                {
                                    if (searchItemID(item, out var content))
                                    {
                                        itemsSource.Add(item);
                                        contents.Add(content);
                                    }
                                }
                                break;
                            case SearchKeyType.SearchName:
                                foreach (var item in items)
                                {
                                    if (searchItemName(item, out var content))
                                    {
                                        itemsSource.Add(item);
                                        contents.Add(content);
                                    }
                                }
                                break;
                            case SearchKeyType.SearchType:
                                foreach (var item in items)
                                {
                                    if (searchItemType(item, out var content))
                                    {
                                        itemsSource.Add(item);
                                        contents.Add(content);
                                    }
                                }
                                break;
                            case SearchKeyType.SearchDescription:
                                foreach (var item in items)
                                {
                                    if (searchItemDesc(item, out var content))
                                    {
                                        itemsSource.Add(item);
                                        contents.Add(content);
                                    }
                                }
                                break;
                            case SearchKeyType.SearchModule:
                                foreach (var item in items)
                                {
                                    if (searchItemMod(item, out var content))
                                    {
                                        itemsSource.Add(item);
                                        contents.Add(content);
                                    }
                                }
                                break;
                        }
                        break;
                    case 2:
                        bool searchTempID(ItemTemplate template, out string content)
                        {
                            content = null;
                            bool result = template.IDPrefix.Contains(keyword);
                            if (result) content = $"{template.Name}\n({Tr("ID前缀")}: {ZetanUtility.Editor.HighlightContentByKey(template.IDPrefix, keyword, template.IDPrefix.Length)})";
                            return result;
                        }
                        bool searchTempName(ItemTemplate template, out string content)
                        {
                            content = null;
                            bool result = template.Name.Contains(keyword);
                            if (result) content = $"{ZetanUtility.Editor.HighlightContentByKey(template.Name, keyword, template.Name.Length)}";
                            return result;
                        }
                        bool searchTempType(ItemTemplate template, out string content)
                        {
                            content = null;
                            var type = ItemTypeEnum.Instance[template.Type];
                            bool result = type.Name.Contains(keyword);
                            if (result) content = $"{template.Name}\n({Tr("默认类型")}: {ZetanUtility.Editor.HighlightContentByKey(type.Name, keyword, type.Name.Length)})";
                            return result;
                        }
                        bool searchTempDesc(ItemTemplate template, out string content)
                        {
                            content = null;
                            bool result = template.Description.Contains(keyword);
                            if (result) content = $"{template.Name}\n({Tr("默认描述")}: {ZetanUtility.Editor.HighlightContentByKey(template.Description, keyword, 30)})";
                            return result;
                        }
                        bool searchTempMod(ItemTemplate template, out string content)
                        {
                            content = null;
                            bool result = template.Modules.Any(x => x.GetName().Contains(keyword));
                            if (result)
                            {
                                content = template.Modules.FirstOrDefault(x => x.GetName().Contains(keyword)).GetName();
                                content = $"{template.Name}\n({Tr("模块")}: {ZetanUtility.Editor.HighlightContentByKey(content, keyword, content.Length)})";
                            }
                            return result;
                        }

                        switch (keyType)
                        {
                            case SearchKeyType.SearchAll:
                                foreach (var template in templates)
                                {
                                    if (searchTempID(template, out var content) || searchTempName(template, out content) || searchTempType(template, out content)
                                        || searchTempDesc(template, out content) || searchTempMod(template, out content))
                                    {
                                        itemsSource.Add(template);
                                        contents.Add(content);
                                    }
                                }
                                break;
                            case SearchKeyType.SearchID:
                                foreach (var template in templates)
                                {
                                    if (searchTempID(template, out var content))
                                    {
                                        itemsSource.Add(template);
                                        contents.Add(content);
                                    }
                                }
                                break;
                            case SearchKeyType.SearchName:
                                foreach (var template in templates)
                                {
                                    if (searchTempName(template, out var content))
                                    {
                                        itemsSource.Add(template);
                                        contents.Add(content);
                                    }
                                }
                                break;
                            case SearchKeyType.SearchType:
                                foreach (var template in templates)
                                {
                                    if (searchTempType(template, out var content))
                                    {
                                        itemsSource.Add(template);
                                        contents.Add(content);
                                    }
                                }
                                break;
                            case SearchKeyType.SearchDescription:
                                foreach (var template in templates)
                                {
                                    if (searchTempDesc(template, out var content))
                                    {
                                        itemsSource.Add(template);
                                        contents.Add(content);
                                    }
                                }
                                break;
                            case SearchKeyType.SearchModule:
                                foreach (var template in templates)
                                {
                                    if (searchTempMod(template, out var content))
                                    {
                                        itemsSource.Add(template);
                                        contents.Add(content);
                                    }
                                }
                                break;
                        }
                        break;
                }
                bindItem = (e, i) =>
                {
                    (e as Label).text = contents[i];
                };
            }
            searchDropdown.itemsSource = itemsSource;
            searchDropdown.bindItem = bindItem;
            searchDropdown.RefreshItems();
        }
        private void OnSearchListSelected(IEnumerable<object> os)
        {
            if (os.FirstOrDefault() is Item item)
                SelectListItem(items.IndexOf(item));
            else if (os.FirstOrDefault() is ItemTemplate template)
                SelectListTemplate(templates.IndexOf(template));
            searchDropdown.SetSelectionWithoutNotify(null);
            searchField.value = null;
        }
        #endregion

        #region 道具相关
        private void SelectListItem(int index)
        {
            itemList.SetSelection(index);
            itemList.ScrollToItem(index);
        }
        private void OnListItemSelected(IEnumerable<Item> items)
        {
            rightPanel.Clear();
            selectedItems = items;
            if (items != null && items.Count() == 1)
            {
                var item = items.FirstOrDefault();
                //if (Item.Editor.ClearInvalidModule(item)) Debug.LogWarning(Tr("道具 [{0}] 存在失效模块，已自动移除。", item.Name));
                selectedItem = item;
                serializedItem = item ? new SerializedObject(item) : null;
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
            RefreshDeleteButton();
            RefreshCloneButton();
        }
        private void RefreshItems()
        {
            items = Item.Editor.GetItems(currentTemplate);
            items.Sort(Item.Comparer.Default.Compare);
            if (itemList != null)
            {
                itemList.itemsSource = items;
                itemList.RefreshItems();
            }
            RefreshDeleteButton();
            RefreshCloneButton();
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
        private ItemModule AddModule(Type type, int index = -1)
        {
            if (type == null || type.IsAbstract || !typeof(ItemModule).IsAssignableFrom(type) || !selectedItem && !selectedTemplate) return null;
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
                    undoName = $"{Tr("添加模块至道具")} {selectedItem.Name}";
                    addModule = () => Item.Editor.AddModule(selectedItem, type, index);
                    scriptableObject = selectedItem;
                    break;
                case 2:
                    serializedObject = serializedTemplate;
                    modules = selectedTemplate.Modules;
                    undoName = $"{Tr("添加模块至模板")} {selectedTemplate.Name}";
                    addModule = () => ItemTemplate.Editor.AddModule(selectedTemplate, type, index);
                    scriptableObject = selectedTemplate;
                    break;
            }
            Type duplicate = null;
            if (!CommonModule.IsCommon(type) && modules.Any(x => ItemModule.Duplicate(x, type, out duplicate)))
                EditorUtility.DisplayDialog(Tr("无法添加"), Tr("已经存在 [{0}] 模块，此类模块只能添加一个。", ItemModule.GetName(duplicate)), Tr("确定"));
            else if (serializedObject != null)
            {
                Undo.RegisterCompleteObjectUndo(scriptableObject, undoName);
                if (addModule() is ItemModule module)
                {
                    RefreshModules();
                    rightPanel.contentContainer.RegisterCallback<GeometryChangedEvent>(scollToEnd);
                    return module;

                    void scollToEnd(GeometryChangedEvent evt)
                    {
                        ModuleBlock block = rightPanel.Query<ModuleBlock>().Where(x => x.userData == module);
                        rightPanel.ScrollTo(block);
                        rightPanel.contentContainer.UnregisterCallback<GeometryChangedEvent>(scollToEnd);
                    }
                }
            }
            return null;
        }
        private void MakeAddModuleButton(IEnumerable<ItemModule> existModules)
        {
            VisualElement buttonArea = new VisualElement();
            buttonArea.style.paddingTop = 7;
            buttonArea.style.paddingBottom = 7;
            rightPanel.Add(buttonArea);
            var types = TypeCache.GetTypesDerivedFrom<ItemModule>().Where(x => !x.IsAbstract && (typeof(CommonModule).IsAssignableFrom(x) || !existModules.Any(y => ItemModule.Duplicate(y, x, out _))));
            Button button = new Button() { text = Tr("添加模块") };
            button.clicked += () =>
            {
                var dropdown = new AdvancedDropdown<Type>(types, t => AddModule(t), ItemModule.GetName,
                                                          iconGetter: t => EditorGUIUtility.FindTexture("cs Script Icon"),
                                                          title: Tr("可用模块"), addCallbacks: (Tr("模块"), newScript));
                dropdown.Show(dropdownRect());

                Rect dropdownRect()
                {
                    var rect = buttonArea.LocalToWorld(button.layout);
                    rect.y += rightPanel.contentContainer.transform.position.y;
                    return rect;
                }

                void newScript()
                {
                    ZetanUtility.Editor.Script.CreateNewScript("NewModule.cs", settings.newScriptFolder, settings.scriptTemplate);
                }
            };
            button.style.width = 230;
            button.style.height = 25;
            button.style.alignSelf = new StyleEnum<Align>(Align.Center);
            buttonArea.Add(button);
        }
        public void RefreshModules()
        {
            if (funcTab.SelectedIndex == 1 && !selectedItem || funcTab.SelectedIndex == 2 && !selectedTemplate) return;
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
                    rightPanel.Insert(rightPanel.childCount - 1, MakeModuleBlock(property, modules[i], i));
                }
            }
        }
        private ModuleBlock MakeModuleBlock(SerializedProperty property, ItemModule module, int index)
        {
            ModuleBlock block = ModuleBlock.Create(property, module);
            block.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                IList<ItemModule> modules = null;
                switch (funcTab.SelectedIndex)
                {
                    case 1: if (selectedItem) modules = selectedItem.Modules; break;
                    case 2: if (selectedTemplate) modules = selectedTemplate.Modules; break;
                }
                if (modules != null)
                {
                    if (module)
                    {
                        int index = modules.IndexOf(module);
                        evt.menu.AppendAction(Tr("重置"), a => ResetModule(module));
                        evt.menu.AppendSeparator();
                        evt.menu.AppendAction(Tr("移除模块"), a => DeleteModule(module));
                        evt.menu.AppendAction(Tr("向上移动"), a => MoveModuleUp(module), index > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                        evt.menu.AppendAction(Tr("向下移动"), a => MoveModuleDown(module), index < modules.Count - 1 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                        evt.menu.AppendAction(Tr("复制模块"), a => CopyModule(module));
                        evt.menu.AppendAction(Tr("粘贴为新模块"), a => PasteModule(modules.IndexOf(module)), canPaste() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                        evt.menu.AppendAction(Tr("粘贴模块值"), a => PasteModule(module), CanPaste(module) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                        evt.menu.AppendSeparator();
                        evt.menu.AppendAction(Tr("编辑脚本"), a => EditScript(module));
                        evt.menu.AppendAction(Tr("编辑Editor脚本"), a => EditScript(block));

                        bool canPaste()
                        {
                            if (copiedModule == null) return false;
                            var type = copiedModule.GetType();
                            return CommonModule.IsCommon(type) || !modules.Any(x => ItemModule.Duplicate(x, type, out _));
                        }
                    }
                    else
                    {
                        switch (funcTab.SelectedIndex)
                        {
                            case 1:
                                evt.menu.AppendAction(Tr("移除"), a => serializedItem?.FindProperty("modules")?.DeleteArrayElementAtIndex(index));
                                break;
                            case 2:
                                evt.menu.AppendAction(Tr("移除"), a => serializedTemplate?.FindProperty("modules")?.DeleteArrayElementAtIndex(index));
                                break;
                        }
                    }
                }
            }));
            return block;
        }
        private void ResetModule(ItemModule module)
        {
            UnityEngine.Object obj = funcTab.SelectedIndex == 1 ? selectedItem : selectedTemplate;
            if (obj)
            {
                Undo.RegisterCompleteObjectUndo(obj, Tr("重置{0}的{1}模块", obj.name, module.GetName()));
                EditorUtility.CopySerializedManagedFieldsOnly(Activator.CreateInstance(module.GetType()), module);
            }
        }
        private void PasteModule(ItemModule destModule)
        {
            UnityEngine.Object obj = funcTab.SelectedIndex == 1 ? selectedItem : selectedTemplate;
            if (obj && CanPaste(destModule))
            {
                Undo.RegisterCompleteObjectUndo(obj, Tr("在{0}上粘贴模块值", obj.name));
                EditorUtility.CopySerializedManagedFieldsOnly(copiedModule, destModule);
            }
        }
        private void PasteModule(int index)
        {
            if (AddModule(copiedModule.GetType(), index) is ItemModule module)
                EditorUtility.CopySerializedManagedFieldsOnly(copiedModule, module);
        }
        private void CopyModule(ItemModule module)
        {
            copiedModule = module.Copy();
        }
        private bool CanPaste(ItemModule module)
        {
            return copiedModule && copiedModule.GetType().IsAssignableFrom(module.GetType());
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
                var attr = type.GetCustomAttribute<ItemModule.RequireAttribute>(true);
                if (attr != null && attr.modules.Contains(module.GetType()))
                {
                    EditorUtility.DisplayDialog("无法移除", $"因为 [{ItemModule.GetName(type)}] 模块依赖于此模块，暂时无法移除。", "确定");
                    return;
                }
            }
            switch (funcTab.SelectedIndex)
            {
                case 1:
                    Undo.RegisterCompleteObjectUndo(selectedItem, Tr("从道具{0}删除模块", selectedItem.Name));
                    if (Item.Editor.RemoveModule(selectedItem, module)) RefreshModules();
                    break;
                case 2:
                    Undo.RegisterCompleteObjectUndo(selectedTemplate, Tr("从模板{0}删除模块", selectedTemplate.Name));
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
                if (ItemTemplate.Editor.ClearInvalidModule(template)) Debug.LogWarning(Tr("模板 [{0}] 存在失效模块，已自动移除。", template.Name));
                selectedTemplate = template;
                serializedTemplate = template ? new SerializedObject(template) : null;
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
            RefreshDeleteButton();
            RefreshCloneButton();
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
            RefreshCloneButton();
        }
        private void RefreshTemplateSelector()
        {
            var templates = ZetanUtility.Editor.LoadAssets<ItemTemplate>();
            templateNames = new List<string>() { "不指定模板" };
            HashSet<string> existNames = new HashSet<string>();
            foreach (var template in templates)
            {
                string name = template.Name;
                int num = 1;
                string uniueName = name;
                while (existNames.Contains(uniueName))
                {
                    uniueName = $"{name} ({L10n.Tr("Repeat")} {num})";
                    num++;
                }
                existNames.Add(uniueName);
                templateNames.Add(uniueName);
            }
            if (templateSelector != null)
            {
                templateSelector.choices = templateNames;
                templateSelector.value = currentTemplate ? currentTemplate.Name : templateNames[0];
            }
        }
        #endregion

        #region 功能页签相关
        public void OnFuncTab(int index)
        {
            switch (index)
            {
                case 1:
                    rootVisualElement.RemoveFromClassList("template");
                    rootVisualElement.AddToClassList("item");
                    if (listLabel != null)
                    {
                        listLabel.text = Tr("道具列表");
                        listLabel.style.backgroundColor = new StyleColor(new Color(0.33f, 0.43f, 0.34f, 1));
                    }
                    rightPanel.Clear();
                    RefreshInspector();
                    searchSelector.choices = itemSearchType;
                    searchSelector.value = Tr("名称");
                    break;
                case 2:
                    rootVisualElement.RemoveFromClassList("item");
                    rootVisualElement.AddToClassList("template");
                    if (listLabel != null)
                    {
                        listLabel.text = Tr("模板列表");
                        listLabel.style.backgroundColor = new StyleColor(new Color(0.33f, 0.40f, 0.43f, 1));
                    }
                    rightPanel.Clear();
                    RefreshInspector();
                    searchSelector.choices = templateSearchType;
                    searchSelector.value = Tr("名称");
                    break;
            }
            RefreshDeleteButton();
            RefreshCloneButton();
        }
        public void OnRightFuncTab(int index, ContextualMenuPopulateEvent evt)
        {
            if (index == funcTab.SelectedIndex)
                evt.menu.AppendAction(Tr("刷新"), a =>
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
        private void RefreshInspector()
        {
            switch (funcTab.SelectedIndex)
            {
                case 1:
                    itemList.RefreshItems();
                    itemList.SetSelection(items.IndexOf(selectedItem));
                    break;
                case 2:
                    templateList.RefreshItems();
                    templateList.SetSelection(templates.IndexOf(selectedTemplate));
                    break;
            }
        }
        #endregion

        #region 新建相关
        private void OnCloneClick()
        {
            switch (funcTab.SelectedIndex)
            {
                case 1: CloneItem(); break;
            }
        }
        private void CloneItem()
        {
            if (!selectedItem) return;
            Item item;
            if (!Item.UseDatabase) item = ZetanUtility.Editor.SaveFilePanel(() => Instantiate(selectedItem), folder: Item.assetsFolder, root: "Resources");
            else item = ItemDatabase.Editor.CloneItem(selectedItem);
            if (item)
            {
                RefreshItems();
                selectedItem = item;

                itemList.RegisterCallback<GeometryChangedEvent>(scollToEnd);

                void scollToEnd(GeometryChangedEvent evt)
                {
                    SelectListItem(items.IndexOf(item));
                    itemList.UnregisterCallback<GeometryChangedEvent>(scollToEnd);
                }
            }
        }
        private void RefreshCloneButton()
        {
            switch (funcTab.SelectedIndex)
            {
                case 1: cloneButton.SetEnabled(selectedItem); break;
                case 2: cloneButton.SetEnabled(selectedTemplate); break;
                default: cloneButton.SetEnabled(false); break;
            }
        }
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
            Item item;
            if (!Item.UseDatabase)
            {
                item = ZetanUtility.Editor.SaveFilePanel(CreateInstance<Item>, folder: Item.assetsFolder, root: "Resources");
                if (item)
                {
                    Item.Editor.ApplyTemplate(item, currentTemplate);
                    Item.Editor.SetAutoID(item, ZetanUtility.Editor.LoadAssets<Item>(), currentTemplate ? currentTemplate.IDPrefix : null);
                    ZetanUtility.Editor.SaveChange(item);
                }
            }
            else item = ItemDatabase.Editor.MakeItem(currentTemplate);
            if (item)
            {
                RefreshItems();
                selectedItem = item;

                itemList.RegisterCallback<GeometryChangedEvent>(scollToEnd);

                void scollToEnd(GeometryChangedEvent evt)
                {
                    SelectListItem(items.IndexOf(item));
                    itemList.UnregisterCallback<GeometryChangedEvent>(scollToEnd);
                }
            }
        }
        private void NewItem(ItemFilterAttribute itemFilter)
        {
            Item item;
            if (!Item.UseDatabase)
            {
                item = ZetanUtility.Editor.SaveFilePanel(CreateInstance<Item>, folder: Item.assetsFolder, root: "Resources");
                if (item)
                {
                    Item.Editor.ApplyFilter(item, itemFilter);
                    if (string.IsNullOrEmpty(item.ID))
                        Item.Editor.SetAutoID(item, ZetanUtility.Editor.LoadAssets<Item>(), currentTemplate ? currentTemplate.IDPrefix : null);
                    ZetanUtility.Editor.SaveChange(item);
                }
            }
            else item = ItemDatabase.Editor.MakeItem(itemFilter);
            if (item)
            {
                RefreshItems();
                selectedItem = item;

                itemList.RegisterCallback<GeometryChangedEvent>(scollToEnd);

                void scollToEnd(GeometryChangedEvent evt)
                {
                    SelectListItem(items.IndexOf(item));
                    itemList.UnregisterCallback<GeometryChangedEvent>(scollToEnd);
                }
            }
        }
        private void NewTemplate()
        {
            ItemTemplate template = ZetanUtility.Editor.SaveFilePanel(CreateInstance<ItemTemplate>);
            if (template)
            {
                templates.Add(template);
                RefreshTemplates();
                RefreshTemplateSelector();

                templateList.RegisterCallback<GeometryChangedEvent>(scollToEnd);

                void scollToEnd(GeometryChangedEvent evt)
                {
                    SelectListTemplate(templates.IndexOf(template));
                    templateList.UnregisterCallback<GeometryChangedEvent>(scollToEnd);
                }

                //this.StartCoroutine(scrollTo());

                //IEnumerator scrollTo()
                //{
                //    yield return new WaitForEndOfFrame();
                //    SelectListTemplate(templates.IndexOf(template));
                //}
            }
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
                        if (!Item.UseDatabase)
                        {
                            if (EditorUtility.DisplayDialog(Tr("删除选中道具"), Tr("确定将选中道具放入回收站吗？"), Tr("确定"), Tr("取消")))
                            {
                                List<string> failedPaths = new List<string>();
                                AssetDatabase.MoveAssetsToTrash(selectedItems.Select(x => AssetDatabase.GetAssetPath(x)).ToArray(), failedPaths);
                            }
                        }
                        else if (EditorUtility.DisplayDialog(Tr("删除选中道具"), Tr("确定将选中道具从数据库中删除吗？此操作将不可逆！"), Tr("确定"), Tr("取消")))
                        {
                            bool success = false;
                            var selectedItems = new List<Item>(this.selectedItems);
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
                        if (EditorUtility.DisplayDialog(Tr("删除选中模板"), Tr("确定将选中模板放入回收站吗？"), Tr("确定"), Tr("取消")))
                        {
                            bool success = false;
                            var selectedTemplates = new List<ItemTemplate>(this.selectedTemplates);
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
                                RefreshTemplateSelector();
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
        private void DeleteItem(Item item)
        {
            if (!Item.UseDatabase)
            {
                if (EditorUtility.DisplayDialog(Tr("删除选中道具"), Tr("确定将道具 [{0}(ID: {1})] 放入回收站吗?", item.Name, item.ID), Tr("确定"), Tr("取消")))
                    AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(item));
            }
            else if (EditorUtility.DisplayDialog(Tr("删除选中道具"), Tr("确定将道具 [{0}(ID: {1})] 从数据库中删除吗? 此操作将不可逆!", item.Name, item.ID), Tr("确定"), Tr("取消")))
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
            if (EditorUtility.DisplayDialog(Tr("删除选中模板"), Tr("确定将模板 [{0}] 放入回收站吗?", template.Name), Tr("确定"), Tr("取消")))
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

        private string Tr(string text, params object[] args)
        {
            return L.Tr(settings.language, text, args);
        }
        private string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }

        private enum SearchKeyType
        {
            SearchAll,
            SearchName,
            SearchID,
            SearchType,
            SearchDescription,
            SearchModule,
        }
    }
}