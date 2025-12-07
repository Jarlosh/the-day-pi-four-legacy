using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Client.App;
using Game.Client.UI;
using Game.Core;
using Game.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Game.Client
{
	public class GameResultScreen: MonoBehaviour
	{
		[Header("References")] [SerializeField]
		private StyleSystem _styleSystem;

		[SerializeField] private HudScreen _hudScreen;

		[Header("UI Elements")] [SerializeField]
		private GameObject _resultPanel;

		[SerializeField] private TextMeshProUGUI _resultTitleText;
		[SerializeField] private TextMeshProUGUI _scoreText;
		[SerializeField] private TextMeshProUGUI _suckedObjectText;
		[SerializeField] private Button _menuButton;

		[Header("Settings")] [SerializeField] private LocalizedString _winTitle;
		[SerializeField] private LocalizedString _defeatTitle;
		[SerializeField] private LocalizedString _scoreFormat;
		[SerializeField] private LocalizedString _suckedObjectsFormat;

		private CancellationTokenSource _cancellationSource = new();
		private bool _isShowing = false;
		private PlayerInput _playerInput;
		private LoadingScreenProvider _loadingScreenProvider = new LoadingScreenProvider();

		private void Awake()
		{
			if (_styleSystem == null)
			{
				_styleSystem = ServiceLocator.Get<StyleSystem>();
			}

			if (_resultPanel != null)
			{
				_resultPanel.SetActive(false);
			}

			_playerInput = FindFirstObjectByType<PlayerInput>();
		}

		private void OnEnable()
		{
			EventBus.Instance.Subscribe<GameCancelEvent>(OnGameEnded);
			if (_menuButton != null)
			{
				_menuButton.onClick.AddListener(OnMenuButtonClicked);
			}
		}

		private void OnDisable()
		{
			EventBus.Instance.Unsubscribe<GameCancelEvent>(OnGameEnded);
			if (_menuButton != null)
			{
				_menuButton.onClick.RemoveListener(OnMenuButtonClicked);
			}
		}

		private void OnGameEnded(GameCancelEvent gameEvent)
		{
			if (_isShowing)
			{
				return;
			}

			_hudScreen.gameObject.SetActive(false);

			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = true;

			_playerInput?.actions.Disable();

			_isShowing = true;
			ShowResult(gameEvent.Results);
		}

		private async void ShowResult(GameResults result)
		{
			if (_resultPanel != null)
			{
				_resultPanel.SetActive(true);
			}

			if (_resultTitleText != null)
			{
				string str;
				
				if (result == GameResults.Win)
				{
					str = await LocalizationUtils.GetLocalizedString(_winTitle);
				}
				else
				{
					str = await LocalizationUtils.GetLocalizedString(_defeatTitle);
				}
				
				_resultTitleText.text = str;

			}

			if (_scoreText != null && _styleSystem != null)
			{
				var totalScore = Mathf.CeilToInt(_styleSystem.TotalScore);
				var str = await LocalizationUtils.GetLocalizedString(_scoreFormat);
				
				_scoreText.text = string.Format(str, totalScore);
			}
			
			if (_suckedObjectsFormat != null && _styleSystem != null)
			{
				var totalScore = Mathf.CeilToInt(_styleSystem.VacuumedObjectsCount);
				var str = await LocalizationUtils.GetLocalizedString(_suckedObjectsFormat);
				
				_suckedObjectText.text = string.Format(str, totalScore);
			}
		}

		private void OnMenuButtonClicked()
		{
			_playerInput?.actions.Enable();

			LoadMainMenu();
		}

		private async void LoadMainMenu()
		{
			CancellationToken token = _cancellationSource.Token;

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