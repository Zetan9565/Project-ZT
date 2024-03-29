namespace ZetanStudio.BehaviourTree.Nodes
{
    [Description("概率成功结点：以一定的概率向上反馈评估成功")]
    public class RandomProbability : Conditional
    {
        [Label("成功率")]
        public SharedFloat successProbability = 0.5f;
        [Label("自定义种子")]
        public SharedBool useSeed;
        [Label("随机种子"), HideIf("useSeed", false)]
        public SharedInt seed;

        private System.Random random;

        public override bool IsValid => successProbability != null && seed != null && useSeed != null && successProbability.IsValid && seed.IsValid && useSeed.IsValid;

        protected override void OnAwake()
        {
            if (useSeed.Value) random = new System.Random(seed.Value);
            else random = new System.Random();
        }

        public override bool CheckCondition()
        {
            float randomValue = (float)random.NextDouble();
            if (randomValue < successProbability.Value)
            {
                return true;
            }
            return false;
        }

        protected override void OnReset()
        {
            successProbability = 0.5f;
            seed = 0;
            useSeed = false;
        }
    }

}