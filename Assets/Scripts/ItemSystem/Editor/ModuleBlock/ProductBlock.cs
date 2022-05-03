using UnityEditor;
using ZetanExtends.Editor;
using ZetanStudio.Item.Module;

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
            list = new DropItemListDrawer(serializedObject, product, lineHeight, lineHeightSpace);
        }

        protected override void OnInspectorGUI()
        {
            if (product.arraySize < 1) EditorGUILayout.PropertyField(productInfo);
            if (productInfo.objectReferenceValue is null) list?.DoLayoutDraw();
        }
    }
}