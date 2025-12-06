using Game.Core;
using TMPro;
using UnityEngine;

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

		[Header("Settings")] [SerializeField] private string _waveFormat = "Wave {0}/{1}";
		[SerializeField] private string _enemiesFormat = "Enemies: {0}";
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
		}

		protected override void Unsubscribe()
		{
			base.Unsubscribe();

			EventBus.Instance.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
			EventBus.Instance.Unsubscribe<WaveCompletedEvent>(OnWaveCompleted);
			EventBus.Instance.Unsubscribe<CountdownEvent>(OnCountdown);
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
					_waveFormat,
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
				_enemiesRemainingText.text = string.Format(_enemiesFormat, count);
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