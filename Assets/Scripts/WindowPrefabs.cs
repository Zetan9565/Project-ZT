using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ZetanStudio.UI
{
    [CreateAssetMenu]
    public class WindowPrefabs : SingletonScriptableObject<WindowPrefabs>
    {
        [SerializeField]
        private Window[] windows = { };
        public ReadOnlyCollection<Window> Windows => new ReadOnlyCollection<Window>(windows);

        public Window GetWindowPrefab(Type type)
        {
            return windows?.FirstOrDefault(x => x && x.GetType() == type);
        }
    }
}