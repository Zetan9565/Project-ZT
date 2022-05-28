using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TypeSearchProvider : ScriptableObject, ISearchWindowProvider
{
    private Type type;
    private IEnumerable<Type> types;
    private string title;
    private Action<Type> selectCallback;
    private Func<Type, string> nameGetter;
    private Func<Type, string> groupGetter;
    private Func<Type, Texture> iconGetter;
    private ScriptMaker newScriptMaker;

    public delegate void ScriptMaker(out string fileName, out string folder, out TextAsset template);

    public static void OpenWindow<T>(SearchWindowContext context,
                                     Action<Type> selectCallback,
                                     IEnumerable<Type> types = null,
                                     string title = null,
                                     Func<Type, string> nameGetter = null,
                                     Func<Type, string> groupGetter = null,
                                     Func<Type, Texture> iconGetter = null,
                                     ScriptMaker newScriptMaker = null)
    {
        SearchWindow.Open(context, Create<T>(selectCallback, types, title, nameGetter, groupGetter, iconGetter, newScriptMaker));
    }

    public static TypeSearchProvider Create<T>(Action<Type> selectCallback,
                                               IEnumerable<Type> types = null,
                                               string title = null,
                                               Func<Type, string> nameGetter = null,
                                               Func<Type, string> groupGetter = null,
                                               Func<Type, Texture> iconGetter = null,
                                               ScriptMaker newScriptMaker = null)
    {
        var instance = CreateInstance<TypeSearchProvider>();
        instance.Init<T>(selectCallback, types, title, nameGetter, groupGetter, iconGetter, newScriptMaker);
        return instance;
    }

    public void Init<T>(Action<Type> selectCallback,
                        IEnumerable<Type> types = null,
                        string title = null,
                        Func<Type, string> nameGetter = null,
                        Func<Type, string> groupGetter = null,
                        Func<Type, Texture> iconGetter = null,
                        ScriptMaker newScriptMaker = null)
    {
        type = typeof(T);
        this.title = title;
        this.types = types;
        this.selectCallback = selectCallback;
        this.nameGetter = nameGetter;
        this.groupGetter = groupGetter;
        this.iconGetter = iconGetter;
        this.newScriptMaker = newScriptMaker;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> treeEntries = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent(title ?? "类型"))
        };
        types ??= TypeCache.GetTypesDerivedFrom(type);
        HashSet<string> groups = new HashSet<string>();
        foreach (var type in types)
        {
            string[] groupContent;
            if (groupGetter != null)
                if (groupGetter(type) is string str) groupContent = str.Split('/');
                else groupContent = new string[0];
            else groupContent = type.Namespace.Split('.') ?? new string[0];
            string group = string.Empty;
            for (int i = 0; i < groupContent.Length; i++)
            {
                group += groupContent[i];
                if (!groups.Contains(group))
                {
                    groups.Add(group);
                    treeEntries.Add(new SearchTreeGroupEntry(new GUIContent(groupContent[i]), i + 1));
                }
                group += "/";
            }
            Texture texture = null;
            if (iconGetter != null) texture = iconGetter(type);
            if (!texture) texture = EditorGUIUtility.FindTexture("cs Script Icon");
            SearchTreeEntry treeEntry = new SearchTreeEntry(new GUIContent(nameGetter?.Invoke(type) ?? type.Name) { image = texture })
            {
                level = groupContent.Length + 1,
                userData = type
            };
            treeEntries.Add(treeEntry);
        }
        if (newScriptMaker != null)
        {
            treeEntries.Add(new SearchTreeGroupEntry(new GUIContent("新建"), 1));
            treeEntries.Add(new SearchTreeEntry(new GUIContent($"新建脚本") { image = EditorGUIUtility.FindTexture("CreateAddNew") }) { level = 2, userData = (Action)delegate { NewScript(); } });
        }
        return treeEntries;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        if (SearchTreeEntry.userData is Type type) selectCallback?.Invoke(type);
        else if (SearchTreeEntry.userData is Action action) action.Invoke();
        return true;
    }

    private void NewScript()
    {
        newScriptMaker.Invoke(out var fileName, out var folder, out var template);
        ZetanUtility.Editor.Script.CreateNewScript(fileName, folder, template);
    }
}