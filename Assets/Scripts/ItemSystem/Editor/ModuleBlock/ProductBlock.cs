using UnityEngine;
using UnityEditor;
using ZetanStudio.Item.Module;
using ZetanStudio.Extension.Editor;

namespace ZetanStudio.Item.Editor
{
    [CustomMuduleDrawer(typeof(ProductModule))]
    public class ProductBlock : ModuleBlock
    {
        private readonly DropItemListDrawer list;
        private readonly SerializedProperty productInfo;
        private readonly SerializedProperty product;

        public ProductBlock(SerializedProperty property, ItemModule module) : base(property, module)
        {
            productInfo = property.FindAutoPropertyRelative("ProductInfo");
            product = property.FindPropertyRelative("product");
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float lineHeightSpace = lineHeight + EditorGUIUtility.standardVerticalSpacing;
            list = new DropItemListDrawer(product, lineHeight, lineHeightSpace);
        }

        protected override void OnInspectorGUI()
        {
            if (product.arraySize < 1) EditorGUILayout.PropertyField(productInfo, new GUIContent("产出配置"));
            if (productInfo.objectReferenceValue is null)
            {
                if(product.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(product.isExpanded, "产出表"))
                list?.DoLayoutDraw();
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
    }
}