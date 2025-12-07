using Game.Core;
using Game.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Game.Client.UI
{
	public class WaveInfoView: UIViewBase
	{
		[Header("References")] [SerializeField]
		private WaveManager _waveManager;

		[Header("UI Elements")] [SerializeField]
		private TextMeshProUGUI _waveNumberText;

		[SerializeField] private TextMeshProUGUI _enemiesRemainingText;
		[SerializeField] private TextMeshProUGUI _timerText;

		[Header("Settings")] 
		[SerializeField] private LocalizedString _waveFormat;
		[SerializeField] private LocalizedString _enemiesFormat;
		[SerializeField] private string _timerFormat = "{0:0}";
		[SerializeField] private bool _showTimer = true;

		private float _countdownTimer;
		private bool _isCountingDown;
		private int _currentEnemiesCount;

		protected override void Init()
		{
			if (_waveManager == null)
			{
				_waveManager = ServiceLocator.Get<WaveManager>();
			}

			UpdateWaveInfo();
			UpdateEnemiesCount(0);

			if (_timerText != null)
			{
				_timerText.gameObject.SetActive(_showTimer);
			}
		}

		protected override void Subscribe()
		{
			base.Subscribe();

			EventBus.Instance.Subscribe<WaveStartedEvent>(OnWaveStarted);
			EventBus.Instance.Subscribe<WaveCompletedEvent>(OnWaveCompleted);
			EventBus.Instance.Subscribe<CountdownEvent>(OnCountdown);
			
			LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
		}

		protected override void Unsubscribe()
		{
			base.Unsubscribe();

			EventBus.Instance.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
			EventBus.Instance.Unsubscribe<WaveCompletedEvent>(OnWaveCompleted);
			EventBus.Instance.Unsubscribe<CountdownEvent>(OnCountdown);
			
			LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
		}

		private void OnSelectedLocaleChanged(Locale locale)
		{
			UpdateWaveInfo();
			UpdateEnemiesCount(_waveManager.GetActiveEnemiesCount());
		}

		private void Update()
		{
			if (_waveManager == null)
			{
				return;
			}

			UpdateEnemiesCount(_waveManager.GetActiveEnemiesCount());

			if (_isCountingDown && _showTimer)
			{
				_countdownTimer -= Time.deltaTime;
				UpdateTimer(_countdownTimer);
			}
		}

		private void OnWaveStarted(WaveStartedEvent waveEvent)
		{
			_isCountingDown = false;
			UpdateWaveInfo();

			if (_timerText != null && _showTimer)
			{
				_timerText.gameObject.SetActive(false);
			}
		}

		private void OnWaveCompleted(WaveCompletedEvent waveEvent)
		{
			UpdateWaveInfo();
		}

		private void OnCountdown(CountdownEvent countdownEvent)
		{
			_isCountingDown = true;
			_countdownTimer = countdownEvent.Seconds;

			if (_timerText != null && _showTimer)
			{
				_timerText.gameObject.SetActive(true);
				UpdateTimer(_countdownTimer);
			}
		}

		private void UpdateWaveInfo()
		{
			if (_waveManager == null)
				return;

			if (_waveNumberText != null)
			{
				_waveNumberText.text = string.Format(
					LocalizationUtils.GetLocalizedString(_waveFormat).ToString(),
					_waveManager.CurrentWaveNumber,
					_waveManager.TotalWaves
				);
			}
		}

		private void UpdateEnemiesCount(int count)
		{
			_currentEnemiesCount = count;

			if (_enemiesRemainingText != null)
			{
				_enemiesRemainingText.text = string.Format(LocalizationUtils.GetLocalizedString(_enemiesFormat).ToString(), count);
			}
		}

		private void UpdateTimer(float time)
		{
			if (_timerText != null)
			{
				_timerText.text = string.Format(_timerFormat, Mathf.CeilToInt(time));
			}
		}
	}
}