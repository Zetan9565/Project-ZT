using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Module;

namespace ZetanStudio
{
    public class EquipmentWindow : Window
    {
        [SerializeField]
        private List<EquipmentSlot> slots = new List<EquipmentSlot>();

        [SerializeField]
        private Text text;

        public bool ContainsSlot(EquipmentSlot slot)
        {
            return slots.Contains(slot);
        }

        protected override void OnAwake()
        {
            foreach (var slot in slots)
            {
                slot.SetCallbacks((s) =>
                {
                    return new ButtonWithTextData[] { new ButtonWithTextData("卸下", () => Unequip(s)) };
                }, Unequip, OnSlotEndDrag);
            }
        }

        private void Unequip(ItemSlotEx slot)
        {
            Unequip(slot.Item);
        }
        protected void OnSlotEndDrag(GameObject go, ItemSlotEx slot)
        {
            if (go.GetComponentInParent<ItemSlotEx>() is ItemSlotEx other && WindowsManager.IsWindowOpen<BackpackWindow>(out var backpack) && backpack.Grid.Contains(other))
            {
                if (other.IsEmpty) Unequip(slot.Item);
                else if (other.Item.GetModule<EquipableModule>().Type == (slot as EquipmentSlot).SlotType)
                    ItemUsage.UseItem(other.Item);
            }
        }

        private void Unequip(ItemData Item)
        {
            if (EquipmentManager.Unequip(Item))
                Refresh();
        }

        protected override bool OnOpen(params object[] args)
        {
            Refresh();
            return true;
        }

        protected override void RegisterNotify()
        {
            NotifyCenter.AddListener(BackpackManager.BackpackItemAmountChanged, (o) => Refresh(), this);
        }

        private void Refresh()
        {
            string t = string.Empty;
            foreach (var p in EquipmentManager.properties)
            {
                if (p.Type.ShowInPanel)
                {
                    t += p;
                    t += "\n";
                }
            }
            text.text = t;
            foreach (var slot in slots)
            {
                EquipmentManager.equiped.TryGetValue(slot.SlotType, out var find);
                slot.SetItem(find);
            }
        }
    }
}
