namespace ZetanStudio
{
    public class FolderAttribute : EnhancedPropertyAttribute
    {
        public readonly string root;
        public readonly bool external;

        public FolderAttribute()
        {

        }
        public FolderAttribute(bool external)
        {
            this.external = external;
        }
        public FolderAttribute(string root)
        {
            this.root = root;
        }
    }
}
