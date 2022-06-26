using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    [CreateAssetMenu(fileName = "take medichine", menuName = "Zetan Studio/道具/用途/吃药")]
    public class TakeMedichine : ItemUsage
    {
        public TakeMedichine()
        {
            _name = "吃药";
        }

        protected override bool Use(ItemData item)
        {
            MessageManager.Instance.New("吃药");
            return true;
        }
    }
}