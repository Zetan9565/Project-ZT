namespace ZetanStudio.BehaviourTree
{
    [System.Serializable]
    public class SharedBoolean : SharedVariable<bool>
    {
        public static implicit operator SharedBoolean(bool value)
        {
            return new SharedBoolean() { value = value };
        }
    }
}