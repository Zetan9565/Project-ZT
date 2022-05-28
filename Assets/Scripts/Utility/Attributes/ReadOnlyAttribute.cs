public class ReadOnlyAttribute : EnhancedPropertyAttribute
{
    public readonly bool onlyRuntime;

    public ReadOnlyAttribute(bool onlyRuntime = false)
    {
        this.onlyRuntime = onlyRuntime;
    }
}