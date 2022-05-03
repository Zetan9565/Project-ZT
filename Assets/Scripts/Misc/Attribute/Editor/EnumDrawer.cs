using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumAttribute))]
public class EnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Integer)
        {
            var attr = (EnumAttribute)attribute;
            foreach (var type in TypeCache.GetTypesDerivedFrom<ScriptableObjectEnum>())
            {
                if (!type.IsAbstract && !type.IsGenericTypeDefinition)
                {
                    var generics = type.BaseType.GetGenericArguments();
                    if (attr.type == generics[1])
                    {
                        var instance = type.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                        if (instance.GetValue(null) is ScriptableObjectEnum Enum)
                        {
                            var types = Enum.GetEnum().Select(x => new GUIContent(x.Name));
                            property.intValue = EditorGUI.Popup(position, label, property.intValue, types.ToArray());
                            return;
                        }
                    }
                }
            }
        }
        EditorGUI.PropertyField(position, property, label);
    }
}