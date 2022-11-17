using UnityEngine;

namespace ZetanStudio.CraftSystem
{

    [CreateAssetMenu(fileName = "tool info", menuName = "Zetan Studio/制作工具信息")]
    public class CraftToolInformation : ScriptableObject
    {
        [SerializeField, Label("工具类型"), Enum(typeof(CraftToolType))]
        private int toolType;
        public CraftToolType ToolType => CraftToolTypeEnum.Instance[toolType];

        [SerializeField, Label("制作耗时")]
        private float makingTime = 5f;
        public float MakingTime => makingTime;

        private static CraftToolInformation handwork;
        public static CraftToolInformation Handwork
        {
            get
            {
                if (!handwork)
                {
                    handwork = CreateInstance<CraftToolInformation>();
                    handwork.toolType = 0;
                    handwork.makingTime = 5;
                }
                return handwork;
            }
        }
    }
}