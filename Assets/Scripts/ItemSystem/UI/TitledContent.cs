using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio
{
    public class TitledContent : MonoBehaviour
    {
        [SerializeField]
        private Text title;
        [SerializeField]
        private Text content;

        public void Init(string title, string content)
        {
            this.title.text = title;
            this.content.text = content;
        }

        public void Clear()
        {
            title.text = string.Empty;
            content.text = string.Empty;
        }
    }
}
