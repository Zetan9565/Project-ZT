using UnityEngine;
using ZetanStudio.CraftSystem.UI;
using ZetanStudio.InventorySystem;
using ZetanStudio.StructureSystem;

namespace ZetanStudio.CraftSystem
{
    [DisallowMultipleComponent]
    public class CraftTool : Structure2D
    {
        public CraftToolInformation ToolInfo { get; private set; }

        public override bool IsInteractive
        {
            get
            {
                return base.IsInteractive && ToolInfo && !WindowsManager.IsWindowOpen<CraftWindow>();
            }
        }

        protected override void OnNotInteractable()
        {
            if (WindowsManager.IsWindowOpen<CraftWindow>(out var making) && making.CurrentTool == this)
                making.Interrupt();
            base.OnNotInteractable();
        }

        public override bool DoManage()
        {
            return WindowsManager.OpenWindowBy<CraftWindow>(this, BackpackManager.Instance);
        }

        protected override void OnInit()
        {
            if (Info.Addendas.Count > 0)
            {
                if (Info.Addendas[0] is CraftToolInformation info)
                    ToolInfo = info;
            }
        }
    }
}