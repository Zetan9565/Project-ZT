using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ZetanStudio.Editor
{
    public class ScriptableObjectSearchProvider : SearchProvider<ScriptableObject>
    {
        public static ScriptableObjectSearchProvider Create(IEnumerable<ScriptableObject> objects, Action<ScriptableObject> selectCallback, string title = null,
                                                            Func<ScriptableObject, string> nameGetter = null, Func<ScriptableObject, string> groupGetter = null,
                                                            Func<ScriptableObject, Texture> iconGetter = null, Comparison<ScriptableObject> comparison = null)
        {
            return Create<ScriptableObjectSearchProvider>(objects, selectCallback, title, nameGetter, groupGetter, iconGetter, comparison);
        }

        public static void OpenWindow(SearchWindowContext context, IEnumerable<ScriptableObject> objects, Action<ScriptableObject> selectCallback, string title = null,
                                      Func<ScriptableObject, string> nameGetter = null, Func<ScriptableObject, string> groupGetter = null,
                                      Func<ScriptableObject, Texture> iconGetter = null, Comparison<ScriptableObject> comparison = null)
        {
            OpenWindow<ScriptableObjectSearchProvider>(context, objects, selectCallback, title, nameGetter, groupGetter, iconGetter, comparison);
        }
    }
}