using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Client.App
{

	public interface ISceneLoader
	{
		UniTask<Scene> LoadScene(string sceneName);
		UniTask<Scene> LoadSingleScene(string sceneName);
		UniTask<Scene> LoadSceneAsync(string sceneName);
	}

	public class SceneLoader: ISceneLoader
	{
		public async UniTask<Scene> LoadScene(string sceneName)
		{
			if (HandlePreloadedScene(sceneName, out Scene scene))
			{
				return scene;
			}

			SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
			await UniTask.NextFrame();

			return SceneManager.GetSceneByName(sceneName);
		}

		public async UniTask<Scene> LoadSingleScene(string sceneName)
		{
			SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
			await UniTask.NextFrame();

			return SceneManager.GetSceneByName(sceneName);
		}

		public async UniTask<Scene> LoadSceneAsync(string sceneName)
		{
			if (HandlePreloadedScene(sceneName, out Scene scene))
			{
				return scene;
			}

			await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			return SceneManager.GetSceneByName(sceneName);
		}

		private bool HandlePreloadedScene(string sceneName, out Scene scene)
		{
			scene = SceneManager.GetSceneByName(sceneName);
			return scene.IsValid();
		}
	}
}