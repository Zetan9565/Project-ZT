public class HideWhenPlayingAttribute : EnhancedPropertyAttribute
{
    public readonly bool readOnly;
    public readonly bool reverse;

    public HideWhenPlayingAttribute(bool readOnly = false, bool reverse=false)
    {
        this.readOnly = readOnly;
        this.reverse = reverse;
    }
}