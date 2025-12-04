using System;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Client.App
{
    public class SceneLoadOperation: ILoadingOperation
    {
        private readonly string _sceneName;

        public string Description => "Loading...";

        private Action<AsyncOperation> _callbackAction;

        public SceneLoadOperation(string sceneName, Action<AsyncOperation> callback = null)
        {
            _sceneName = sceneName;
            _callbackAction = callback;
        }

        public async UniTask Load(Action<float> onProgress)
        {
            onProgress?.Invoke(0.1f);

            var loadOperation = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            loadOperation.allowSceneActivation = false;

            while (loadOperation.isDone)
            {
                await UniTask.Delay(1);
            }

            _callbackAction?.Invoke(loadOperation);
            onProgress?.Invoke(1f);
        }
    }
}