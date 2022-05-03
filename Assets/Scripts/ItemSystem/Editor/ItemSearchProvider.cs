using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace ZetanStudio.Item
{
    public class ItemSearchProvider : ScriptableObjectSearchProvider<ItemBase>
    {
        public static ItemSearchProvider Create(IEnumerable<ItemBase> objects, Action<ItemBase> selectCallback)
        {
            return Create<ItemSearchProvider>(objects, selectCallback, "选择道具", i => i.Name, i => ZetanUtility.GetInspectorName(i.ItemType), i => i.Icon ? i.Icon.texture : null, ItemComparer.Default.Compare);
        }
        public static void OpenWindow(SearchWindowContext context, IEnumerable<ItemBase> objects, Action<ItemBase> selectCallback)
        {
            OpenWindow<ItemSearchProvider>(context, objects, selectCallback, "选择道具", i => i.Name, i => ZetanUtility.GetInspectorName(i.ItemType), i => i.Icon ? i.Icon.texture : null, ItemComparer.Default.Compare);
        }
    }
}