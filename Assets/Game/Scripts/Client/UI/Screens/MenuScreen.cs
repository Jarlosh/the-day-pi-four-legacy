using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Client.App;
using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Client.UI
{
	public class MenuScreen: MonoBehaviour
	{
		[field: SerializeField] public Canvas HudCanvas { get; private set; }

		[field: Space]
		[field: SerializeField] internal Button ContinueButton { get; private set; }

		[field: SerializeField] internal Button BackToMainMenuButton { get; private set; }

		[field: Space] 
		[field: SerializeField] private InputActionReference ResumeInputAction { get; set; }

		private ITimeService _timeService;
		private LoadingScreenProvider _loadingScreenProvider = new LoadingScreenProvider();
		private CancellationTokenSource _cancellationSource = new();
		private PlayerInput _playerInput;

		private bool _toMenu;

		private void Awake()
		{
			_timeService = ServiceLocator.Get<ITimeService>();
			
			if (_playerInput == null)
			{
				_playerInput = FindFirstObjectByType<PlayerInput>();
			}
		}

		private void OnDestroy()
		{
			_playerInput = null;
			_timeService = null;
		}

		private void OnEnable()
		{
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = true;
			
			ResumeInputAction.action.performed += OnResumeInputPerformed;

			BackToMainMenuButton.onClick.AddListener(OnBackToMainMenuHandler);
			ContinueButton.onClick.AddListener(OnContinueHandler);

			_timeService.Pause(this, PauseType.Menu);
			_playerInput?.actions.Disable();
		}
		
		private void OnDisable()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			if(!_toMenu)
			{
				_playerInput?.actions.Enable();
				_timeService.Resume(this);
			}

			BackToMainMenuButton.onClick.RemoveListener(OnBackToMainMenuHandler);
			ContinueButton.onClick.RemoveListener(OnContinueHandler);

			ResumeInputAction.action.performed -= OnResumeInputPerformed;
		}

		private void OnResumeInputPerformed(InputAction.CallbackContext context)
		{
			ResumeGame();
		}

		private void OnContinueHandler()
		{
			ResumeGame();
		}

		private void ResumeGame()
		{
			HudCanvas.gameObject.SetActive(true);
			gameObject.SetActive(false);
		}

		private void OnBackToMainMenuHandler()
		{
			_timeService.Resume(this);
			ResumeInputAction.action.performed -= OnResumeInputPerformed;
			
			LoadMainMenu();
		}
		
		private async void LoadMainMenu()
		{
			CancellationToken token = _cancellationSource.Token;
			_toMenu = true;
			
			AsyncOperation mainOperation = new AsyncOperation();
			await LoadGameScene();

			token.ThrowIfCancellationRequested();

			mainOperation.allowSceneActivation = true;

			await UniTask.NextFrame(token);

			async UniTask LoadGameScene()
			{
				var operations = new Queue<ILoadingOperation>();
				operations.Enqueue(new SingleSceneLoadOperation(ApplicationSettings.MainMenuScene, OperationCallback));

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