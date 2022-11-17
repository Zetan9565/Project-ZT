using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.UI
{
    public class FloatButtonPanel : Window
    {
        [SerializeField]
        private ButtonWithTextList buttonList;

        protected override bool OnOpen(params object[] args)
        {
            if (args.Length > 0 && args[0] is IEnumerable<ButtonWithTextData> datas)
            {
                buttonList.Refresh(datas);
                if (args.Length > 1 && args[1] is Vector2 position)
                    buttonList.transform.position = position;
                else buttonList.transform.position = Utility.ScreenCenter;
                Utility.KeepInsideScreen(buttonList.RectTransform);
                return true;
            }
            return false;
        }
    }
}