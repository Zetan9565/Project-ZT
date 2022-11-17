using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio
{
    public class LoadingPanel : SingletonMonoBehaviour<LoadingPanel>
    {
        [SerializeField]
        private Slider slider;
        [SerializeField]
        private Text text;

        public static void UpdateProgress(float progress)
        {
            if (Instance)
            {
                progress = Mathf.Clamp01(progress);
                if (Instance.slider) Instance.slider.value = progress;
                if (Instance.text) Instance.text.text = progress.ToString("p1");
            }
        }
    }
}
