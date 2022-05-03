using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ZetanStudio.Character
{
    [CreateAssetMenu]
    public class RoleAttributeEnum : SingletonScriptableObject<RoleAttributeEnum>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<string> names = new List<string>();
        [SerializeField]
        private List<RoleAttributeValueType> types = new List<RoleAttributeValueType>();

        private Dictionary<string, RoleAttributeValueType> attributeTypes = new Dictionary<string, RoleAttributeValueType>();
        public ReadOnlyDictionary<string, RoleAttributeValueType> AttributeTypes => new ReadOnlyDictionary<string, RoleAttributeValueType>(attributeTypes);

        public void OnAfterDeserialize()
        {
            attributeTypes.Clear();

            for (int i = 0; i < Mathf.Min(names.Count, types.Count); i++)
            {
                attributeTypes.Add(names[i], types[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            names.Clear();
            types.Clear();

            foreach (var attr in attributeTypes)
            {
                names.Add(attr.Key);
                types.Add(attr.Value);
            }
        }
    }
}