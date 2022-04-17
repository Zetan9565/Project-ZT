public abstract class InteractionWindow<T> : Window where T : IInteractive
{
    public abstract T Target { get; }
    public bool hidePanelOnInteract;

    public void Interrupt()
    {
        OnInterrupt();
        Close();
    }
    protected virtual void OnInterrupt() { }

    protected override bool OnOpen(params object[] args)
    {
        if (hidePanelOnInteract) InteractionPanel.Instance.ShowOrHidePanelBy(Target, false);
        return true;
    }

    /// <summary>
    /// 派生类重写时，应优先调用<see cref="InteractionWindow{T}.OnClose(object[])"/>，再进行后续操作
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    protected override bool OnClose(params object[] args)
    {
        if (hidePanelOnInteract) InteractionPanel.Instance.ShowOrHidePanelBy(Target, true);
        if (Target != null) Target.EndInteraction();
        return true;
    }
}