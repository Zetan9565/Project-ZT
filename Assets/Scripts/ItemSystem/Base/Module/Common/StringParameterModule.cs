namespace ZetanStudio.Item.Module
{
    [Name("字符串参数")]
    public class StringParameterModule : CommonModule<string>
    {
        public override bool IsValid => base.IsValid && !string.IsNullOrEmpty(Parameter);
    }
}