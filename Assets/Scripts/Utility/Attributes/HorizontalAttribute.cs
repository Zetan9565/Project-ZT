public class HorizontalAttribute : EnhancedPropertyAttribute
{
    public readonly int position;
    public readonly int count;

    public HorizontalAttribute(int position, int count)
    {
        this.position = position;
        this.count = count;
    }
}