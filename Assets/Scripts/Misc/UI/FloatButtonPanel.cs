using System.Collections.Generic;
using UnityEngine;

public class FloatButtonPanel : Window
{
    [SerializeField]
    private ButtonWithTextList buttonList;

    protected override bool OnOpen(params object[] args)
    {
        var datas = args[0] as IEnumerable<ButtonWithTextData>;
        if (args.Length > 1)
            buttonList.Refresh(datas);
        return true;
    }
}
