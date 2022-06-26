public abstract class Transition
{

}
public class StringTransition : Transition
{
    public readonly string value;

    public StringTransition(string value)
    {
        this.value = value;
    }
}
public class BoolTransition : Transition
{
    public readonly bool value;

    public BoolTransition(bool value)
    {
        this.value = value;
    }
}
public class IntTransition : Transition
{
    public readonly int value;

    public IntTransition(int value)
    {
        this.value = value;
    }
}
public class FloatTransition : Transition
{
    public readonly float value;

    public FloatTransition(float value)
    {
        this.value = value;
    }
}
public class ObjectTransition : Transition
{
    public readonly object value;

    public ObjectTransition(object value)
    {
        this.value = value;
    }
}