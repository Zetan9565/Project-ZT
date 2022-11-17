using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.Editor
{
    public class ScriptableObjectManager : EditorWindow
    {
        private ScriptableObjectManagerSettings settings;
        private Button create;
        private Button delete;
        private VisualElement dropdown;
        private ListView list;
        private IMGUIContainer inspector;
        private List<ScriptableObject> allObjects;
        private List<ScriptableObject> objects;
        private string fullTypeName;
        private Type currentType;
        private string TypeName => currentType?.Name ?? null;
        private ScriptableObject selected;
        private IEnumerable<ScriptableObject> selecteds;

        [MenuItem("Window/Zetan Studio/工具/ScriptableObject管理")]
        private static void CreateWindow()
        {
            ScriptableObjectManager wnd = GetWindow<ScriptableObjectManager>();
            ScriptableObjectManagerSettings settings = ScriptableObjectManagerSettings.GetOrCreate();
            wnd.minSize = settings.minWindowSize;
            wnd.titleContent = new GUIContent(L.Tr(settings.language, "ScriptableObject管理器"));
        }

        public void CreateGUI()
        {
            try
            {
                settings = settings ? settings : ScriptableObjectManagerSettings.GetOrCreate();

                VisualElement root = rootVisualElement;
                var visualTree = settings.treeUxml;
                visualTree.CloneTree(root);
                var styleSheet = settings.treeUss;
                root.styleSheets.Add(styleSheet);

                create = root.Q<Button>("create");
                create.clicked += Create;
                create.SetEnabled(false);
                delete = root.Q<Button>("delete");
                delete.clicked += Delete;
                delete.SetEnabled(false);
                root.Q<Button>("create-script").clicked += NewScript;

                dropdown = root.Q<VisualElement>("dropdown-area");
                var dropdownIns = new IMGUIContainer(() =>
                {
                    var rect = EditorGUILayout.GetControlRect();
                    if (GUI.Button(rect, string.IsNullOrEmpty(TypeName) ? "未选择" : TypeName, EditorStyles.popup))
                    {
                        var types = TypeCache.GetTypesDerivedFrom<ScriptableObject>().Where(valid);

                        static bool valid(Type type)
                        {
                            return type.Assembly.FullName.Contains("Assembly-CSharp")
                            && !type.IsAbstract
                            && !typeof(UnityEditor.Editor).IsAssignableFrom(type)
                            && !typeof(EditorWindow).IsAssignableFrom(type)
                            && !typeof(StateMachineBehaviour).IsAssignableFrom(type)
                            && !typeof(ISearchWindowProvider).IsAssignableFrom(type);
                        }
                        var dropdown = new AdvancedDropdown<Type>(types, OnTypeSelect, t => t.Name, groupGetter, title: Tr("类型"));
                        dropdown.Show(rect);
                    }

                    static string groupGetter(Type type)
                    {
                        return string.IsNullOrEmpty(type.Namespace) ? string.Empty : $"{type.Namespace.Replace('.', '/')}";
                    }
                });
                dropdown.Add(dropdownIns);

                list = root.Q<ListView>();
                list.selectionType = SelectionType.Multiple;
                list.makeItem = () =>
                {
                    var label = new Label();
                    label.AddManipulator(new ContextualMenuManipulator(evt =>
                    {
                        if (label.userData is ScriptableObject item)
                        {
                            evt.menu.AppendAction(Tr("定位"), a => EditorGUIUtility.PingObject(item));
                            evt.menu.AppendAction(Tr("删除"), a => Delete(item));
                        }
                    }));
                    return label;
                };
                list.bindItem = (e, i) =>
                {
                    (e as Label).text = objects[i].name;
                    e.userData = objects[i];
                };
                list.onSelectionChange += (os) => OnListItemSelected(os.Select(x => x as ScriptableObject));

                inspector = new IMGUIContainer();
                root.Q<VisualElement>("right-container").Add(inspector);

                if (!string.IsNullOrEmpty(fullTypeName)) currentType = Type.GetType(fullTypeName);
                RefreshList();
                list.SetSelection(objects.IndexOf(selected));
                list.RegisterCallback<GeometryChangedEvent>(scrollTo);

                void scrollTo(GeometryChangedEvent evt)
                {
                    var index = objects.IndexOf(selected);
                    list.SetSelection(index);
                    list.ScrollToItem(index);
                    list.UnregisterCallback<GeometryChangedEvent>(scrollTo);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        void OnTypeSelect(Type type)
        {
            if (type != currentType)
            {
                currentType = type;
                fullTypeName = type.AssemblyQualifiedName;
                create.SetEnabled(type.GetCustomAttribute<CreateAssetMenuAttribute>() != null);
                inspector.onGUIHandler = null;
                if (inspector.userData is UnityEditor.Editor e) DestroyImmediate(e);
                list.ClearSelection();
                RefreshList();
            }
        }

        private void OnListItemSelected(IEnumerable<ScriptableObject> objects)
        {
            inspector.onGUIHandler = null;
            if (inspector.userData is UnityEditor.Editor e) DestroyImmediate(e);
            selected = null;
            selecteds = objects;
            if (objects.Count() == 1)
            {
                selected = objects.Single();
                UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(selected);
                foreach (var field in editor.GetType().GetFields(Utility.CommonBindingFlags))
                {
                    if (isAnima(field.FieldType))
                    {
                        var anima = field.GetValue(editor);
                        var onValueChanged = anima.GetType().GetField("valueChanged", Utility.CommonBindingFlags).GetValue(anima) as UnityEngine.Events.UnityEvent;
                        onValueChanged.RemoveListener(Repaint);
                        onValueChanged.AddListener(Repaint);
                    }
                }
                inspector.userData = editor;
                inspector.onGUIHandler = editor.OnInspectorGUI;

                static bool isAnima(Type type)
                {
                    return typeof(AnimBool) == type || typeof(AnimFloat) == type || typeof(AnimVector3) == type || typeof(AnimQuaternion) == type;
                }
            }
            delete.SetEnabled(selecteds.Count() > 0);
        }

        private void Delete(ScriptableObject item)
        {
            if (EditorUtility.DisplayDialog(Tr("删除选中资源"), Tr("确定将 [{0}] 放入回收站吗?", item.name), Tr("确定"), Tr("取消")))
            {
                if (AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(item)))
                {
                    RefreshList();
                    inspector.Clear();
                }
            }
        }

        private void RefreshList()
        {
            allObjects = Utility.Editor.LoadAssets<ScriptableObject>("Assets");
            objects = allObjects.FindAll(x => x.GetType() == currentType);
            objects.Sort((x, y) =>
            {
                return string.Compare(x.name, y.name);
            });
            list.itemsSource = objects;
            list.RefreshItems();
        }

        private void Create()
        {
            ScriptableObject so = Utility.Editor.SaveFilePanel(() => CreateInstance(currentType), "New " + ObjectNames.NicifyVariableName(currentType.Name));
            if (so)
            {
                RefreshList();

                list.RegisterCallback<GeometryChangedEvent>(scollToEnd);

                void scollToEnd(GeometryChangedEvent evt)
                {
                    var index = objects.IndexOf(so);
                    list.SetSelection(index);
                    list.ScrollToItem(index);
                    list.UnregisterCallback<GeometryChangedEvent>(scollToEnd);
                }
            }
        }

        private void Delete()
        {
            if (selecteds != null && EditorUtility.DisplayDialog(Tr("删除选中资源"), Tr("确定将选中资源放入回收站吗？"), Tr("确定"), Tr("取消")))
            {
                List<string> failedPaths = new List<string>();
                AssetDatabase.MoveAssetsToTrash(selecteds.Select(x => AssetDatabase.GetAssetPath(x)).ToArray(), failedPaths);
                inspector.Clear();
            }
        }

        void NewScript()
        {
            Utility.Editor.SaveFolderPanel(path =>
            {
                Utility.Editor.Script.CreateNewScript("NewScriptableObject.cs", path, settings.scriptTemplate);
            }, settings.newScriptFolder);
        }

        private void OnProjectChange()
        {
            if (objects.Exists(x => !x))
                RefreshList();
        }

        private void OnDestroy()
        {
            if (inspector?.userData is UnityEditor.Editor e) DestroyImmediate(e);
        }

        private string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }
        private string Tr(string text, params object[] args)
        {
            return L.Tr(settings.language, text, args);
        }
    }
}