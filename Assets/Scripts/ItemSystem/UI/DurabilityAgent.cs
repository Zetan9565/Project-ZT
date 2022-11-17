using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.ItemSystem.UI
{
    public class DurabilityAgent : MonoBehaviour
    {
        public Image fill;

        public Text value;

        public void UnShow()
        {
            Utility.SetActive(gameObject, false);
        }
    }
}