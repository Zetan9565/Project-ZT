﻿using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ZetanStudio.Editor
{
    public abstract class SearchProvider<T> : ScriptableObject, ISearchWindowProvider where T : class
    {
        private List<T> objects;
        private Action<T> selectCallback;
        private Func<T, string> nameGetter;
        private Func<T, string> groupGetter;
        private Func<T, Texture> iconGetter;
        private string title;

        public static TP Create<TP>(IEnumerable<T> objects, Action<T> selectCallback, string title = null,
                                    Func<T, string> nameGetter = null, Func<T, string> groupGetter = null,
                                    Func<T, Texture> iconGetter = null, Comparison<T> comparison = null) where TP : SearchProvider<T>
        {
            var instance = CreateInstance<TP>();
            instance.Init(objects, selectCallback, title, nameGetter, groupGetter, iconGetter, comparison);
            return instance;
        }

        public static void OpenWindow<TP>(SearchWindowContext context, IEnumerable<T> objects, Action<T> selectCallback, string title = null,
                                          Func<T, string> nameGetter = null, Func<T, string> groupGetter = null,
                                          Func<T, Texture> iconGetter = null, Comparison<T> comparison = null) where TP : SearchProvider<T>
        {
            SearchWindow.Open(context, Create<TP>(objects, selectCallback, title, nameGetter, groupGetter, iconGetter, comparison));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objects"><typeparamref name="T"/>列表</param>
        /// <param name="title">窗口标题</param>
        /// <param name="nameGetter">名字提取器</param>
        /// <param name="groupGetter">分组提取器，可用'/'隔开，如：“防具/上衣”</param>
        /// <param name="comparison">排序比较器</param>
        public void Init(IEnumerable<T> objects, Action<T> selectCallback, string title = null,
                         Func<T, string> nameGetter = null, Func<T, string> groupGetter = null,
                         Func<T, Texture> iconGetter = null, Comparison<T> comparison = null)
        {
            this.objects = new List<T>(objects);
            if (comparison != null) this.objects.Sort(comparison);
            this.title = title;
            this.selectCallback = selectCallback;
            this.nameGetter = nameGetter;
            this.groupGetter = groupGetter;
            this.iconGetter = iconGetter;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> treeEntries = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent(title ?? typeof(T).Name))
        };
            HashSet<string> groups = new HashSet<string>();
            foreach (var obj in objects)
            {
                string[] groupContent;
                if (groupGetter != null && groupGetter(obj) is string str) groupContent = str.Split('/');
                else groupContent = new string[0];
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
                if (iconGetter != null) texture = iconGetter(obj);
                if (!texture && obj is UnityEngine.Object o) texture = Utility.Editor.GetIconForObject(o);
                SearchTreeEntry treeEntry = new SearchTreeEntry(new GUIContent(nameGetter?.Invoke(obj) ?? obj.ToString()) { image = texture })
                {
                    level = groupContent.Length + 1,
                    userData = obj
                };
                treeEntries.Add(treeEntry);
            }
            return treeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            selectCallback?.Invoke(SearchTreeEntry.userData as T);
            return true;
        }
    }
}