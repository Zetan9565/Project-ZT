using UnityEngine;
using UnityEditor;
using ZetanStudio.ItemSystem.Module;
using ZetanStudio.Extension.Editor;

namespace ZetanStudio.ItemSystem.Editor
{
    [CustomMuduleDrawer(typeof(ProductModule))]
    public class ProductBlock : ModuleBlock
    {
        private readonly SerializedProperty productInfo;
        private readonly SerializedProperty product;

        public ProductBlock(SerializedProperty property, ItemModule module) : base(property, module)
        {
            productInfo = property.FindAutoProperty("ProductInfo");
            product = property.FindPropertyRelative("product");
        }

        protected override void OnInspectorGUI()
        {
            if (product.arraySize < 1) EditorGUILayout.PropertyField(productInfo, new GUIContent(Tr("公共产出表")));
            if (productInfo.objectReferenceValue is null) EditorGUILayout.PropertyField(product, new GUIContent(Tr("产出表")));
        }
    }
}