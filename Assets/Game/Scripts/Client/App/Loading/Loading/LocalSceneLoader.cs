using System.Threading.Tasks;
using Game.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Client.App
{
	public class LocalSceneLoader: SceneLoader
	{
		private Scene _cachedScene;
		private AsyncOperation _asyncOperation;

		protected async Task<T> LoadingInternal<T>(string sceneName)
		{
			var loadedScene = await LoadSceneAsync(sceneName);
			var component = loadedScene.FindComponent<T>();

			_cachedScene = loadedScene;

			return component;
		}

		protected void UnloadInternal()
		{
			Debug.Log($"Unloading scene: {_cachedScene.name}");

			SceneManager.UnloadSceneAsync(_cachedScene);

			Debug.Log($"{_cachedScene.name} unloaded");
		}
	}
}