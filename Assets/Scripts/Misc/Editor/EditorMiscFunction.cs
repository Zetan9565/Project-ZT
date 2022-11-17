using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.Editor
{
    public static class EditorMiscFunction
    {
        public static void OpenKeywordsSelection(Action<string> callback, Vector2 position)
        {
            List<ScriptableObject> objects = new List<ScriptableObject>();
            foreach (var method in TypeCache.GetMethodsWithAttribute<GetKeywordsMethodAttribute>())
            {
                try
                {
                    objects.AddRange(method.Invoke(null, null) as IEnumerable<ScriptableObject>);
                }
                catch { }
            }
            var dropdown = new AdvancedDropdown<ScriptableObject>(new Vector2(200, 300), objects, s => makeKeywords(s as IKeyword), s => (s as IKeyword).Name, s => getGroup(s as IKeyword), title: L10n.Tr("Keywords"));
            dropdown.Show(position);

            void makeKeywords(IKeyword obj) => callback?.Invoke(Keyword.Generate(obj));

            static string getGroup(IKeyword obj)
            {
                var type = obj.GetType();
                string group = type.Name + "/";
                if (type.GetCustomAttribute<KeywordsGroupAttribute>() is KeywordsGroupAttribute attr)
                {
                    group = attr.group;
                    group = group.EndsWith('/') ? group : group + '/';
                }
                group += obj.Group;
                return group;
            }
        }

        public static void SetAsKeywordsField(TextField text)
        {
            text.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                var index = text.cursorIndex;
                if (index == text.selectIndex)
                    evt.menu.AppendAction(EDL.Tr("插入关键字"), a =>
                    {
                        OpenKeywordsSelection(k =>
                        {
                            text.value = text.value.Insert(index, k);
                            EditorApplication.delayCall += () => text.SelectRange(index, index + k.Length);
                        }, a.eventInfo.mousePosition);
                    });
                var input = typeof(TextField).GetProperty("textInput", Utility.CommonBindingFlags).GetValue(text) as VisualElement;
                evt.target = input;
                input.GetType().BaseType.GetMethod("BuildContextualMenu", Utility.CommonBindingFlags).Invoke(input, new object[] { evt });
            }));
        }
    }
}