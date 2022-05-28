using System;
using UnityEngine;

public class ObjectSelectorAttribute : PropertyAttribute
{
    public readonly Type type;
    public readonly string memberAsName;
    public readonly string memberAsGroup;
    public readonly string memberAsTooltip;
    public readonly string resPath;
    public readonly string nameNull;
    public readonly string title;
    public readonly bool displayNone;
    public readonly bool displayAdd;
    public readonly string extension;
    public readonly bool ignorePackages;

    public ObjectSelectorAttribute(Type type, string memberAsName = null, string memberAsGroup = null,
                                   string memberAsTooltip = null, string resPath = null, string nameNull = null,
                                   string title = null, bool displayNone = false, bool displayAdd = false, string extension = null, bool ignorePackages = true)
    {
        this.type = type;
        this.memberAsName = memberAsName;
        this.memberAsGroup = memberAsGroup;
        this.memberAsTooltip = memberAsTooltip;
        this.resPath = resPath;
        this.nameNull = nameNull;
        this.title = title;
        this.displayNone = displayNone;
        this.displayAdd = displayAdd;
        this.extension = extension;
        this.ignorePackages = ignorePackages;
    }
    public ObjectSelectorAttribute(string memberAsName = null, string memberAsGroup = null,
                                   string memberAsTooltip = null, string resPath = null, string nameNull = null,
                                   string title = null, bool displayNone = false, bool displayAdd = false, string extension = null, bool ignorePackages = true)
    {
        this.memberAsName = memberAsName;
        this.memberAsGroup = memberAsGroup;
        this.memberAsTooltip = memberAsTooltip;
        this.resPath = resPath;
        this.nameNull = nameNull;
        this.title = title;
        this.displayNone = displayNone;
        this.displayAdd = displayAdd;
        this.extension = extension;
        this.ignorePackages = ignorePackages;
    }
}