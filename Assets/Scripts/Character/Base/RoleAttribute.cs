using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System;

[System.Serializable]
public class RoleAttribute
{
    [SerializeField]
    private RoleAttributeType type;
    public RoleAttributeType Type => type;

    [SerializeField]
    private int intValue;
    public int IntValue => IntValue;

    [SerializeField]
    private float floatValue;
    public float FloatValue => floatValue;

    [SerializeField]
    private bool boolValue;
    public bool BoolValue => boolValue;

    public string name
    {
        get
        {
            switch (type)
            {
                case RoleAttributeType.HP:
                    return "血";
                case RoleAttributeType.MP:
                    return "蓝";
                case RoleAttributeType.SP:
                    return "耐";
                case RoleAttributeType.ATK:
                    return "攻";
                case RoleAttributeType.DEF:
                    return "防";
                case RoleAttributeType.ATKSpd:
                    return "攻速";
                case RoleAttributeType.MoveSpd:
                    return "移速";
                default:
                    return "未知属性";
            }
        }
    }

    public object Value
    {
        get
        {
            switch (type)
            {
                case RoleAttributeType.HP:
                case RoleAttributeType.MP:
                case RoleAttributeType.SP:
                case RoleAttributeType.ATK:
                case RoleAttributeType.DEF:
                    return intValue;
                case RoleAttributeType.Hit:
                case RoleAttributeType.Crit:
                case RoleAttributeType.ATKSpd:
                case RoleAttributeType.MoveSpd:
                    return floatValue;
                default:
                    return intValue;
            }
        }
    }

    public static Type AttributeType2ValueType(RoleAttributeType type)
    {
        switch (type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.ATK:
            case RoleAttributeType.DEF:
                return typeof(int);
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpd:
            case RoleAttributeType.MoveSpd:
                return typeof(float);
            default:
                return typeof(int);
        }
    }
}

public enum RoleAttributeType
{
    HP,
    MP,
    SP,
    ATK,
    DEF,
    Hit,
    Crit,
    ATKSpd,
    MoveSpd,
}

[System.Serializable]
public class RoleAttributeGroup
{
    [SerializeField, NonReorderable]
    private List<RoleAttribute> attributes = new List<RoleAttribute>();

    public List<RoleAttribute> Attributes => attributes;

    public object GetValueByType(RoleAttributeType type)
    {
        if (RoleAttribute.AttributeType2ValueType(type).Equals(typeof(int)))
        {
            int tempInt = 0;
            foreach (RoleAttribute attr in attributes)
            {
                if (attr.Type == type)
                {
                    tempInt += attr.IntValue;
                }
            }
            return tempInt;
        }
        else if (RoleAttribute.AttributeType2ValueType(type).Equals(typeof(float)))
        {
            float tempFloat = 0;
            foreach (RoleAttribute attr in attributes)
            {
                if (attr.Type == type)
                {
                    tempFloat += attr.FloatValue;
                }
            }
            return tempFloat;
        }
        return 0;
    }
}

public class RoleAttributeGroupDrawer
{
    private readonly SerializedObject owner;

    public ReorderableList AttrsList { get; }

    public RoleAttributeGroupDrawer(SerializedObject owner, SerializedProperty property, float lineHeight, float lineHeightSpace, string listTitle = "属性列表")
    {
        this.owner = owner;
        SerializedProperty attrs = property.FindPropertyRelative("attributes");
        if (attrs != null)
        {
            AttrsList = new ReorderableList(owner, attrs, true, true, true, true);
            AttrsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                owner.Update();
                EditorGUI.BeginChangeCheck();
                SerializedProperty attr = attrs.GetArrayElementAtIndex(index);
                SerializedProperty type = attr.FindPropertyRelative("type");
                SerializedProperty value = attr.FindPropertyRelative(TypeToPropertyName(type.enumValueIndex));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width / 2, lineHeight), type, new GUIContent(string.Empty));
                EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, lineHeight), value, new GUIContent(string.Empty));
                if (EditorGUI.EndChangeCheck())
                    owner.ApplyModifiedProperties();
            };

            AttrsList.elementHeightCallback = (index) =>
            {
                return lineHeightSpace;
            };

            AttrsList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, listTitle);
            };

            AttrsList.drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "空列表");
            };
        }
    }

    public void DrawLayoutEditor()
    {
        owner.Update();
        EditorGUI.BeginChangeCheck();
        if (AttrsList != null)
            AttrsList.DoLayoutList();
        if (EditorGUI.EndChangeCheck())
            owner.ApplyModifiedProperties();
    }

    public void DrawEditor(Rect rect)
    {
        owner.Update();
        EditorGUI.BeginChangeCheck();
        if (AttrsList != null)
            AttrsList.DoList(rect);
        if (EditorGUI.EndChangeCheck())
            owner.ApplyModifiedProperties();
    }

    private string TypeToPropertyName(int type)
    {
        switch ((RoleAttributeType)type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.ATK:
            case RoleAttributeType.DEF:
                return "intValue";
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpd:
            case RoleAttributeType.MoveSpd:
                return "floatValue";
            default:
                return "intValue";
        }
    }
}