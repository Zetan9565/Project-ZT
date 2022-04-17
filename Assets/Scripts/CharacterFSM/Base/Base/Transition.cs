public abstract class Transition
{
    public readonly string name;

    public Transition(string name)
    {
        this.name = name;
    }
}
public class BoolTransition : Transition
{
    public readonly bool value;

    public BoolTransition(string name, bool value) : base(name)
    {
        this.value = value;
    }

    public static readonly BoolTransition relive = new BoolTransition("relive", true);
}
public class IntTransition : Transition
{
    public readonly int value;

    public IntTransition(string name, int value) : base(name)
    {
        this.value = value;
    }
}
public class FloatTransition : Transition
{
    public readonly float value;

    public FloatTransition(string name, float value) : base(name)
    {        this.value = value;
    }
}
public class ObjectTransition : Transition
{
    public readonly object value;

    public ObjectTransition(string name, object value) : base(name)
    {
        this.value = value;
    }
}