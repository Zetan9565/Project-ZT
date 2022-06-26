using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.ItemSystem.UI
{
    using Extension;
    using Module;
    using System;

    public class EnhancementWindow : Window
    {
        [SerializeField]
        private ItemSlotEx icon;

        [SerializeField]
        private GameObject consumableContainer;
        [SerializeField]
        private ItemSlotEx costIcon;

        [SerializeField]
        private GameObject materialsContainer;
        [SerializeField]
        private Dropdown setSelector;
        [SerializeField]
        private ScrollMaterialList matList;

        [SerializeField]
        private GameObject experience;

        [SerializeField]
        private Button enhance;

        private ItemData item;
        private EnhancementModule module;
        private EnhancementData data;
        private ItemData cost;
        private EnhLevelConsumables consumables;
        private EnhConsumable consumable;
        private EnhLevelMaterials materials;
        private EnhMaterialSet materialSet;
        private BackpackWindow backpack;
        private ISlotContainer slotContainer;

        protected override void OnAwake()
        {
            enhance.onClick.AddListener(Enhance);
            matList.SetItemModifier(x => x.handler = BackpackManager.Instance);
            setSelector.onValueChanged.AddListener(SetMaterials);
            icon.SetCallbacks(s => new ButtonWithTextData[] { new ButtonWithTextData(Tr("取出"), ResetItem) }, s => ResetItem(), OnIconDragPut);
            costIcon.SetCallbacks(s => new ButtonWithTextData[] { new ButtonWithTextData(Tr("取出"), () => SetConsumable(null)) }, s => SetConsumable(null), OnCostIconDragPut);
        }
        private void OnCostIconDragPut(GameObject go, ItemSlotEx slot)
        {
            if (slotContainer.Contains(go.GetComponent<ItemSlot>()))
                SetConsumable(null);
        }
        private void OnIconDragPut(GameObject go, ItemSlotEx slot)
        {
            if (slotContainer.Contains(go.GetComponent<ItemSlot>()))
                ResetItem();
        }
        public void Enhance()
        {
            WindowsManager.CloseWindow<ItemWindow>();
            bool success = false;
            IEnumerable<CountedItem> loseItems = null;
            switch (module.Method)
            {
                case EnhanceMethod.SingleItem:
                    if (BackpackManager.Instance.GetAmount(cost) < consumable.Amount)
                    {
                        MessageManager.Instance.New(Tr("材料不足"));
                        return;
                    }
                    success = ZetanUtility.Probability(consumable.SuccessRate);
                    loseItems = new CountedItem[] { new CountedItem(cost, consumable.Amount) };
                    break;
                case EnhanceMethod.Materials:
                    if (!BackpackManager.Instance.IsMaterialsEnough(matList.Datas))
                    {
                        MessageManager.Instance.New(Tr("材料不足"));
                        return;
                    }
                    success = ZetanUtility.Probability(materialSet.SuccessRate);
                    loseItems = BackpackManager.Instance.GetMaterials(matList.Datas);
                    break;
                case EnhanceMethod.Experience:
                    break;
                default:
                    break;
            }
            if (!BackpackManager.Instance.CanLose(loseItems)) return;
            if (success)
            {
                EnhanceAttributes();
                EnhanceAffix();
                data.level++;
                MessageManager.Instance.New(Tr("强化成功"));
            }
            else MessageManager.Instance.New(Tr("强化失败"));
            BackpackManager.Instance.Lose(loseItems);
            Refresh();
        }
        private void EnhanceAttributes()
        {
            var incre = module.GenerateIncrements(data.level + 1);
            var dict = new Dictionary<string, ItemProperty>();
            var attr = item.GetModuleData<AttributeData>();
            foreach (var a in attr.properties)
            {
                dict[a.ID] = a;
            }
            foreach (var i in incre)
            {
                if (dict.TryGetValue(i.ID, out var find)) find.Plus(i.Value);
                else attr.properties.Add(i);
            }
        }
        private void EnhanceAffix()
        {
            if (item.TryGetModule<AffixModule>(out var affix) && item.TryGetModule<AffixEnhancementModule>(out var affixEnhan))
            {
                var data = item.GetModuleData<AffixData>();
                var incre = affixEnhan.GenerateIncrements(affix.UpperLimit, data.affixes, 1);
                var dict = new Dictionary<string, ItemProperty>();
                foreach (var a in data.affixes)
                {
                    dict[a.ID] = a;
                }
                foreach (var i in incre)
                {
                    if (dict.TryGetValue(i.ID, out var find)) find.Plus(i.Value);
                    else data.affixes.Add(i);
                }
            }
        }

        public bool SetItem(ItemData item)
        {
            if ((module = item.GetModule<EnhancementModule>()) is null) return false;
            if ((data = item.GetModuleData<EnhancementData>()) is null) return false;
            this.item = item;
            Refresh();
            return true;
        }
        public bool SetConsumable(ItemData item)
        {
            if (!item)
            {
                ResetConsumable();
                return false;
            }
            if (!item.GetModule<EnhConsumableModule>()) return false;
            int have = BackpackManager.Instance.GetAmount(item);
            if (have <= 0)
            {
                ResetConsumable();
                return false;
            }
            cost = item;
            consumable = consumables.Materials.FirstOrDefault(x => x.Item == cost.Model);
            costIcon.SetItem(item, MiscFuntion.GetColorAmountString(have, consumable.Amount));
            enhance.interactable = have >= consumable.Amount;
            return true;
        }
        public void ResetConsumable()
        {
            cost = null;
            consumable = null;
            costIcon.Vacate();
            enhance.interactable = false;
        }
        public void Refresh()
        {
            if (!item)
            {
                ResetItem();
                return;
            }
            icon.SetItem(item);
            enhance.interactable = false;
            if (!data.IsMax)
            {
                ZetanUtility.SetActive(consumableContainer, module.Method == EnhanceMethod.SingleItem);
                ZetanUtility.SetActive(materialsContainer, module.Method == EnhanceMethod.Materials);
                ZetanUtility.SetActive(experience, module.Method == EnhanceMethod.Experience);
                switch (module.Method)
                {
                    case EnhanceMethod.SingleItem:
                        RefreshConsumable();
                        break;
                    case EnhanceMethod.Materials:
                        RefreshMaterials();
                        break;
                    case EnhanceMethod.Experience:
                        break;
                    default:
                        break;
                }
            }
            else Prepare();
        }

        private void RefreshMaterials()
        {
            materials = module.Materials[data.level];
            setSelector.ClearOptions();
            for (int i = 0; i < materials.Materials.Count; i++)
            {
                setSelector.options.Add(new Dropdown.OptionData("材料一"));
            }
            setSelector.RefreshShownValue();
            SetMaterials(setSelector.value);
            slotContainer.DarkIf(null);
        }

        private void Prepare()
        {
            module = null;
            consumable = null;
            consumables = null;
            cost = null;
            materialSet = null;
            icon.Vacate();
            costIcon.Vacate();
            matList.Clear();
            setSelector.SetValueWithoutNotify(0);
            ZetanUtility.SetActive(consumableContainer, false);
            ZetanUtility.SetActive(materialsContainer, false);
            ZetanUtility.SetActive(experience, false);
            slotContainer.DarkIf(x => !x.IsEmpty && !EnhancementModule.IsEnhanceable(x.Item));
        }

        private void RefreshConsumable()
        {
            if (!item) return;
            consumables = module.Costs[data.level];
            slotContainer.DarkIf(x => consumables.Materials.None(y => y.Item == x.Data.Model));
            SetConsumable(cost);
        }

        private void SetMaterials(int set)
        {
            if (!item) return;
            materialSet = materials.Materials[set];
            matList.Refresh(materialSet.Materials);
            enhance.interactable = matList.Count > 0;
        }

        protected override bool OnOpen(params object[] args)
        {
            backpack = WindowsManager.UnhideOrOpenWindow<BackpackWindow>();
            slotContainer = backpack.Grid as ISlotContainer;
            if (args.Length > 0 && args[0] is ItemData item) SetItem(item);
            else ResetItem();
            return true;
        }

        private void ResetItem()
        {
            item = null;
            Prepare();
        }

        protected override bool OnClose(params object[] args)
        {
            ResetItem();
            slotContainer.DarkIf(null);
            return true;
        }
    }
}
