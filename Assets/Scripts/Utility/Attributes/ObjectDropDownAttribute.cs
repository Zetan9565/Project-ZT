using System;

public class ObjectDropDownAttribute : EnhancedPropertyAttribute
{
    public readonly Type type;
    public readonly string memberAsName;
    public readonly string resPath;
    public readonly string nameNull;

    public ObjectDropDownAttribute(Type type, string memberAsName = "", string resPath = "", string nameNull = "未选择")
    {
        this.type = type;
        this.memberAsName = memberAsName;
        this.resPath = resPath;
        this.nameNull = nameNull;
    }
    public ObjectDropDownAttribute(string memberAsName = "", string resPath = "", string nameNull = "未选择")
    {
        this.memberAsName = memberAsName;
        this.resPath = resPath;
        this.nameNull = nameNull;
    }
}