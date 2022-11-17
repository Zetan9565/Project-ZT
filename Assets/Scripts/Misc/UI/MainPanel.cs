using UnityEngine;
using ZetanStudio;

public sealed class MainPanel : SingletonWindow<MainPanel>, IHideable
{
    [SerializeField]
    private Joystick joystick;
    public Joystick Joystick => joystick;

    public override bool IsOpen => true;
    public bool IsHidden { get; private set; }

    public void Hide(bool hide, params object[] args)
    {
        IHideable.HideHelper(content, hide);
        IsHidden = hide;
    }

    protected override bool OnOpen(params object[] args) => false;
    protected override bool OnClose(params object[] args) => false;
}