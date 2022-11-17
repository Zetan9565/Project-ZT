using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZetanStudio
{
    [AddComponentMenu(null)]
    public sealed class SceneLoader : SingletonMonoBehaviour<SceneLoader>
    {
        public static event Action<string> OnLoadBegin;
        public static event Action<float> OnLoading;
        public static event Action<string> OnLoadDone;
        private static Coroutine coroutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (!Instance) DontDestroyOnLoad(new GameObject(typeof(SceneLoader).Name, typeof(SceneLoader)));
        }

        public static void LoadScene(string name, Action callback = null)
        {
            if (!Instance || coroutine != null) return;
            coroutine = Instance.StartCoroutine(LoadAsync(name, callback));
        }

        private static IEnumerator LoadAsync(string name, Action callback)
        {
            var async = SceneManager.LoadSceneAsync("Loading");
            async.allowSceneActivation = false;
            yield return new WaitUntil(() => async.progress >= 0.9f);
            async.allowSceneActivation = true;
            yield return new WaitUntil(() => async.isDone);
            async = SceneManager.LoadSceneAsync(name);
            async.allowSceneActivation = false;
            OnLoadBegin?.Invoke(name);
            while (async.progress < 0.9f)
            {
                LoadingPanel.UpdateProgress(async.progress / 0.9f);
                OnLoading?.Invoke(async.progress);
                yield return null;
            }
            async.allowSceneActivation = true;
            yield return new WaitUntil(() => async.isDone);
            coroutine = null;
            callback?.Invoke();
            OnLoadDone?.Invoke(name);
        }
    }
}