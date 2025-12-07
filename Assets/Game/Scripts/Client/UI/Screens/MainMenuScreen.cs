using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Client.App;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client
{
	public class MainMenuScreen: MonoBehaviour
	{
		[SerializeField] private Image _fadeScreen;
		[SerializeField] private float _fadeDuration = 1f;
		
		private LoadingScreenProvider _loadingScreenProvider = new LoadingScreenProvider();
		private CancellationTokenSource _gameCancellationSource = new();

		private void Start()
		{
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = true;
		}

		public async void OnStartGameButton()
		{
			_fadeScreen.gameObject.SetActive(true);
			 _fadeScreen.DOFade(1f, _fadeDuration);
        
			await UniTask.WaitForSeconds(_fadeDuration);
        
			LoadGame();
		}

		public void OnQuitButton()
		{
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
       			UnityEngine.Application.Quit();
			#endif
		}

		private async void LoadGame()
		{
			CancellationToken token = _gameCancellationSource.Token;

			AsyncOperation mainOperation = new AsyncOperation();
			await LoadGameScene();

			token.ThrowIfCancellationRequested();

			mainOperation.allowSceneActivation = true;

			await UniTask.NextFrame(token);

			async UniTask LoadGameScene()
			{
				var operations = new Queue<ILoadingOperation>();
				operations.Enqueue(new SingleSceneLoadOperation(ApplicationSettings.GameScene, OperationCallback));

				var loadingScreen = await _loadingScreenProvider.Load();
				await loadingScreen.Load(operations);

				void OperationCallback(AsyncOperation operation)
				{
					mainOperation = operation;

					_loadingScreenProvider.Unload();
				}
			}
		}
	}
}