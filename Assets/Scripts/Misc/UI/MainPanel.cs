using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class MainPanel : SingletonWindow<MainPanel>, IHideable
{
    [SerializeField]
    private Joystick joyStick;
    public Joystick JoyStick => joyStick;

    public override bool IsOpen => true;
    public bool IsHidden { get; private set; }

    public void Hide(bool hide, params object[] args)
    {
        IHideable.HideHelper(content, hide);
        IsHidden = hide;
    }

    protected override bool OnOpen(params object[] args)
    {
        return false;
    }
    protected override bool OnClose(params object[] args)
    {
        return false;
    }
}