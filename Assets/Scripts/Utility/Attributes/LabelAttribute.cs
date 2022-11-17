public sealed class LabelAttribute : EnhancedPropertyAttribute
{
    public readonly string name;

    public LabelAttribute(string name)
    {
        this.name = name;
    }
}