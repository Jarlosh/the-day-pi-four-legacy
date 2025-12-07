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
		private int _maxEnemiesInWave = 0;
		private int _enemiesKilled = 0;

		protected override void Init()
		{
			if (_waveManager == null)
			{
				_waveManager = ServiceLocator.Get<WaveManager>();
			}

			UpdateWaveInfo();
			UpdateEnemiesCount();

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
			EventBus.Instance.Subscribe<EnemyDeathEvent>(OnEnemyDeath);
			
			LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
		}

		protected override void Unsubscribe()
		{
			base.Unsubscribe();

			EventBus.Instance.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
			EventBus.Instance.Unsubscribe<WaveCompletedEvent>(OnWaveCompleted);
			EventBus.Instance.Unsubscribe<CountdownEvent>(OnCountdown);
			EventBus.Instance.Unsubscribe<EnemyDeathEvent>(OnEnemyDeath);
			
			LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
		}

		private void OnSelectedLocaleChanged(Locale locale)
		{
			UpdateWaveInfo();
			UpdateEnemiesCount();
		}

		private void Update()
		{
			if (_waveManager == null)
			{
				return;
			}

			UpdateEnemiesCount();

			if (_isCountingDown && _showTimer)
			{
				_countdownTimer -= Time.deltaTime;
				UpdateTimer(_countdownTimer);
			}
		}

		private void OnWaveStarted(WaveStartedEvent waveEvent)
		{
			_isCountingDown = false;
			_maxEnemiesInWave = waveEvent.MaxEnemiesInWave;
			_enemiesKilled = 0;
			UpdateWaveInfo();
			UpdateEnemiesCount();

			if (_timerText != null && _showTimer)
			{
				_timerText.gameObject.SetActive(false);
			}
		}

		private void OnWaveCompleted(WaveCompletedEvent waveEvent)
		{
			UpdateWaveInfo();
		}
		
		private void OnEnemyDeath(EnemyDeathEvent deathEvent)
		{
			_enemiesKilled++;
			UpdateEnemiesCount();
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

		private async void UpdateWaveInfo()
		{
			if (_waveManager == null)
				return;

			if (_waveNumberText != null)
			{
				var str = await LocalizationUtils.GetLocalizedString(_waveFormat);
				_waveNumberText.text = string.Format(str
					.ToString(),
					_waveManager.CurrentWaveNumber,
					_waveManager.TotalWaves
				);
			}
		}

		private async void UpdateEnemiesCount()
		{
			int remaining = Mathf.Max(0, _maxEnemiesInWave - _enemiesKilled);

			if (_enemiesRemainingText != null)
			{
				var str = await LocalizationUtils.GetLocalizedString(_enemiesFormat);
				_enemiesRemainingText.text = string.Format(str, remaining);
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