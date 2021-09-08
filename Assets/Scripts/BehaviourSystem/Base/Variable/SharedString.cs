namespace ZetanStudio.BehaviourTree
{
    [System.Serializable]
    public class SharedString : SharedVariable<string>
    {
        public static implicit operator SharedString(string value)
        {
            return new SharedString() { value = value };
        }
    }
}