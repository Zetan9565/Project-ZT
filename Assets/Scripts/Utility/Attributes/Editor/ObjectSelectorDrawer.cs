using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomPropertyDrawer(typeof(ObjectSelectorAttribute)), CustomPropertyDrawer(typeof(ScriptableObject), true)]
public class ObjectSelectorDrawer : PropertyDrawer
{
    private IEnumerable<Object> objects;
    private readonly static string[] candidateNames = { "Name", "_Name", "_name" };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ObjectSelectorAttribute attribute = this.attribute as ObjectSelectorAttribute;
        attribute ??= fieldInfo.GetCustomAttribute<ObjectSelectorAttribute>();
        var type = attribute?.type ?? fieldInfo.FieldType;
        if ((attribute?.type == null || fieldInfo.FieldType.IsAssignableFrom(attribute.type)) && typeof(Object).IsAssignableFrom(type))
        {
            objects ??= ZetanUtility.Editor.LoadAssets(type, attribute?.resPath, attribute?.extension, attribute?.ignorePackages ?? true);
            if (attribute != null)
            {
                Draw(position, property, label, type, objects, attribute.memberAsName, attribute.memberAsGroup,
                     attribute.memberAsTooltip, attribute.nameNull, attribute.title, attribute.displayNone,
                     attribute.displayAdd);
            }
            else Draw(position, property, label, type, objects, displayNone: true, displayAdd: true);
        }
        else EditorGUI.PropertyField(position, property, label);
    }

    public static void Draw(Rect position, SerializedProperty property, GUIContent label, System.Type type,
                            IEnumerable<Object> objects, string memberAsName = null, string memberAsGroup = null,
                            string memberAsTooltip = null, string nameNull = null, string title = null,
                            bool displayNone = false, bool displayAdd = false)
    {
        Draw(type, () => GetDropdown(property, memberAsName, memberAsGroup, memberAsTooltip, objects,
                               string.IsNullOrEmpty(title) ? label.text : title, displayNone, displayAdd, type),
                               position, property, label, memberAsName, memberAsTooltip, nameNull);
    }
    public static void Draw<T>(Rect position, SerializedProperty property, GUIContent label, IEnumerable<T> objects,
                               string memberAsName = null, string memberAsGroup = null, string memberAsTooltip = null,
                               string nameNull = null, string title = null, bool displayNone = false,
                               bool displayAdd = false) where T : Object
    {
        var type = typeof(T);
        Draw(type, () => GetDropdown(property, memberAsName, memberAsGroup, memberAsTooltip, objects,
                               string.IsNullOrEmpty(title) ? label.text : title, displayNone, displayAdd, type),
                               position, property, label, memberAsName, memberAsTooltip, nameNull);
    }

    private static AdvancedDropdown GetDropdown<T>(SerializedProperty property, string memberAsName,
                                                   string memberAsGroup, string memberAsTooltip, IEnumerable<T> objects,
                                                   string title, bool displayNone, bool displayAdd, System.Type type) where T : Object
    {
        bool showAdd = displayAdd && typeof(ScriptableObject).IsAssignableFrom(type)
                       && (type.GetCustomAttribute<CreateAssetMenuAttribute>() != null || TypeCache.GetTypesDerivedFrom(type).Any(x => x.GetCustomAttribute<CreateAssetMenuAttribute>() != null));
        var dropdown = new AdvancedDropdown<T>(objects, i => { property.objectReferenceValue = i; property.serializedObject.ApplyModifiedProperties(); },
                                               i => GetName(memberAsName, i), i => GetGroup(memberAsGroup, i), ZetanUtility.Editor.GetIconForObject,
                                               i => GetTooltip(memberAsName, memberAsTooltip, i), title: title,
                                               addCallbacks: showAdd ? addCallback() : default);
        dropdown.displayNone = displayNone;
        return dropdown;

        (string, System.Action)[] addCallback()
        {
            List<(string, System.Action)> callbacks = new List<(string, System.Action)>();
            {
                var menu = type.GetCustomAttribute<CreateAssetMenuAttribute>();
                if (!type.IsAbstract && menu != null)
                    callbacks.Add((menu.menuName?.Split('/')?[^1] ?? type.Name, () => AddCallback(property, type, menu?.fileName ?? $"new {Regex.Replace(type.Name, "([a-z])([A-Z])", "$1 $2").ToLower()}")));
            }
            foreach (var t in TypeCache.GetTypesDerivedFrom(type))
            {
                var tt = t;
                var m = tt.GetCustomAttribute<CreateAssetMenuAttribute>();
                if (m == null) continue;
                var n = m?.menuName?.Split('/')?[^1] ?? tt.Name;
                callbacks.Add((n, () => AddCallback(property, tt, m?.fileName ?? $"new {Regex.Replace(t.Name, "([a-z])([A-Z])", "$1 $2").ToLower()}")));
            }
            return callbacks.ToArray();
        }
    }
    private static void Draw(System.Type type, System.Func<AdvancedDropdown> getDropdown, Rect position, SerializedProperty property, GUIContent label, string memberAsName, string memberAsTooltip, string nameNull)
    {
        bool emptyLable = string.IsNullOrEmpty(label.text);
        float labelWidth = emptyLable ? 0 : EditorGUIUtility.labelWidth;
        Rect labelRect = new Rect(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.BeginProperty(labelRect, label, property);
        EditorGUI.LabelField(labelRect, label);
        EditorGUI.EndProperty();
        var buttonRect = new Rect(position.x + (emptyLable ? 0 : labelWidth + 2), position.y, position.width - labelWidth - (emptyLable ? 0 : 2), EditorGUIUtility.singleLineHeight);
        label = EditorGUI.BeginProperty(buttonRect, label, property);
        if (property.objectReferenceValue) EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
        string name = property.objectReferenceValue ? GetName(memberAsName, property.objectReferenceValue) : nameNull ?? $"{L10n.Tr("None")} ({type.Name})";
        if (EditorGUI.DropdownButton(buttonRect,
                                     new GUIContent(name)
                                     {
                                         image = ZetanUtility.Editor.GetIconForObject(property.objectReferenceValue),
                                         tooltip = GetTooltip(memberAsName, memberAsTooltip, property.objectReferenceValue)
                                     },
                                     FocusType.Keyboard))
        {
            HashSet<string> existNames = new HashSet<string>();
            getDropdown().Show(buttonRect);
        }
        EditorGUI.EndProperty();
        EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
        if (buttonRect.Contains(Event.current.mousePosition))
        {
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                    if (DragAndDrop.objectReferences.Length == 1 && type.IsAssignableFrom(DragAndDrop.objectReferences[0].GetType()))
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    break;
                case EventType.DragPerform:
                    if (DragAndDrop.objectReferences.Length == 1 && type.IsAssignableFrom(DragAndDrop.objectReferences[0].GetType()))
                    {
                        property.objectReferenceValue = DragAndDrop.objectReferences[0];
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    break;
                case EventType.DragExited:
                    DragAndDrop.visualMode = DragAndDropVisualMode.None;
                    break;
            }
        }

        static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.objectReferenceValue is null)
                return;

            menu.AddItem(EditorGUIUtility.TrTextContent("Location"), false, () =>
            {
                EditorGUIUtility.PingObject(property.objectReferenceValue);
            });
            menu.AddItem(EditorGUIUtility.TrTextContent("Select"), false, () =>
            {
                EditorGUIUtility.PingObject(property.objectReferenceValue);
                Selection.activeObject = property.objectReferenceValue;
            });
            menu.AddItem(EditorGUIUtility.TrTextContent("Properties..."), false, () =>
            {
                EditorUtility.OpenPropertyEditor(property.objectReferenceValue);
            });
        }
    }
    private static void AddCallback(SerializedProperty property, System.Type type, string assetName)
    {
        var obj = ZetanUtility.Editor.SaveFilePanel(() => ScriptableObject.CreateInstance(type), assetName, ping: true);
        if (obj)
        {
            property.objectReferenceValue = obj;
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.OpenPropertyEditor(obj);
        }
    }
    private static string GetName(string memberAsName, Object obj)
    {
        string name = obj.name;
        int i = 0;
        if (string.IsNullOrEmpty(memberAsName))
        {
            memberAsName = candidateNames[0];
            i = 1;
        }
        else i = candidateNames.Length;
        for (; i < candidateNames.Length + 1; i++)
        {
            if (obj.GetType().GetField(memberAsName, ZetanUtility.CommonBindingFlags) is FieldInfo field)
            {
                name = field.GetValue(obj)?.ToString();
                break;
            }
            else if (obj.GetType().GetProperty(memberAsName, ZetanUtility.CommonBindingFlags) is PropertyInfo property)
            {
                name = property.GetValue(obj)?.ToString();
                break;
            }
            else if (obj.GetType().GetMethod(memberAsName, ZetanUtility.CommonBindingFlags) is MethodInfo method && method.ReturnType == typeof(string) && method.GetParameters().Where(x => !x.IsOptional).Count() < 1)
            {
                name = method.Invoke(obj, null)?.ToString();
                break;
            }
            if (i < candidateNames.Length) memberAsName = candidateNames[i];
        }
        name = string.IsNullOrEmpty(name) ? obj.name : name;
        return name;
    }
    private static string GetGroup(string memberAsGroup, Object obj)
    {
        string group = string.Empty;
        if (!string.IsNullOrEmpty(memberAsGroup))
        {
            if (obj.GetType().GetField(memberAsGroup, ZetanUtility.CommonBindingFlags) is FieldInfo field) group = field.GetValue(obj).ToString();
            else if (obj.GetType().GetProperty(memberAsGroup, ZetanUtility.CommonBindingFlags) is PropertyInfo property) group = property.GetValue(obj).ToString();
            else if (obj.GetType().GetMethod(memberAsGroup, ZetanUtility.CommonBindingFlags) is MethodInfo method && method.ReturnType == typeof(string) && method.GetParameters().Where(x => !x.IsOptional).Count() < 1)
                group = method.Invoke(obj, null).ToString();
        }
        return group;
    }
    private static string GetTooltip(string memberAsName, string memberAsTooltip, Object obj)
    {
        if (!obj) return null;
        string name = GetName(memberAsName, obj);
        string tooltip = name;
        if (!string.IsNullOrEmpty(memberAsTooltip))
        {
            if (obj.GetType().GetField(memberAsTooltip, ZetanUtility.CommonBindingFlags) is FieldInfo field) tooltip = field.GetValue(obj).ToString();
            else if (obj.GetType().GetProperty(memberAsTooltip, ZetanUtility.CommonBindingFlags) is PropertyInfo property) tooltip = property.GetValue(obj).ToString();
            else if (obj.GetType().GetMethod(memberAsTooltip, ZetanUtility.CommonBindingFlags) is MethodInfo method && method.ReturnType == typeof(string) && method.GetParameters().Length < 1)
                tooltip = method.Invoke(obj, null).ToString();
        }
        if (tooltip == name) tooltip = $"{L10n.Tr("Name")}: {name}";
        return tooltip;
    }
}