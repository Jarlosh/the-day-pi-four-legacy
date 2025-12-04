using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Client.App
{
    public class SingleSceneLoadOperation: ILoadingOperation
    {
        private readonly string _sceneName;
        private Action<AsyncOperation> _callbackAction;

        public string Description => "Loading...";

        public SingleSceneLoadOperation(string sceneName, Action<AsyncOperation> callback = null)
        {
            _sceneName = sceneName;
            _callbackAction = callback;
        }

        public async UniTask Load(Action<float> onProgress)
        {
            onProgress?.Invoke(0.1f);

            var loadOperation = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Single);
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