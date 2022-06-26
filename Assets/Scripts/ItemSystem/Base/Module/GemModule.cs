namespace ZetanStudio.ItemSystem.Module
{
    [Name("宝石"), Require(typeof(UsableModule), typeof(AttributeModule))]
    public class GemModule : ItemModule
    {
        public override bool IsValid => true;
    }
}