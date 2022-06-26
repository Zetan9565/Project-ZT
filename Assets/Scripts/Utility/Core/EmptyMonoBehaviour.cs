using System;
using UnityEngine;

namespace ZetanStudio
{
    public class EmptyMonoBehaviour : MonoBehaviour
    {
        public event Action StartCallback;
        public event Action UpdateCallback;
        public event Action LateUpdateCallback;
        public event Action FixedUpdateCallback;

        public static EmptyMonoBehaviour Singleton { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateSingleton()
        {
            if (!Singleton)
            {
                var go = new GameObject("SingletonMonoBehaviour", typeof(EmptyMonoBehaviour));
                Singleton = go.GetComponent<EmptyMonoBehaviour>();
                DontDestroyOnLoad(go);
            }
        }

        private void Start()
        {
            StartCallback?.Invoke();
        }

        private void Update()
        {
            UpdateCallback?.Invoke();
        }

        private void LateUpdate()
        {
            LateUpdateCallback?.Invoke();
        }

        private void FixedUpdate()
        {
            FixedUpdateCallback?.Invoke();
        }
    }
}