using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Shared;
using UnityEngine;

namespace Game.Client
{
	[DefaultExecutionOrder(-100)]
	public class StyleSystem: MonoBehaviour
	{
		[Header("Config")] 
		[SerializeField, ExposedScriptableObject(false)] private StyleRankSettings _styleSettings;

		[Header("Settings")] 
		[SerializeField] private float _pointsPerHit = 10f;
		[SerializeField] private float _pointsPerKill = 50f;
		[SerializeField] private float _decayDelay = 3f;
		[SerializeField] private float _decaySpeed = 0.5f;
		[SerializeField] private float _pointsToNextRank = 100f;

		private float _currentMeter = 0f;
		private int _currentRankIndex = 0;
		private float _totalPoints = 0f;
		private float _totalScore = 0f;
		private float _decayTimer = 0f;
		private bool _isDecaying = false;
		private CancellationTokenSource _decayCts;

		public float CurrentMeter => _currentMeter;
		public int CurrentRankIndex => _currentRankIndex;
		public float TotalScore => _totalScore;
		public StyleRank CurrentRank => _styleSettings != null ? _styleSettings.GetRank(_currentRankIndex) : null;

		private void Awake()
		{
			if (_styleSettings == null)
			{
				Debug.LogError("StyleRankConfig не назначен!");
			}
			
			ServiceLocator.Register(this);
		}

		private void Start()
		{
			StartDecayTimer().Forget();
		}

		private void OnDestroy()
		{
			_decayCts?.Cancel();
			_decayCts?.Dispose();
		}

		private async UniTaskVoid StartDecayTimer()
		{
			_decayCts = new CancellationTokenSource();
			var token = _decayCts.Token;

			try
			{
				while (!token.IsCancellationRequested)
				{
					await UniTask.Yield(token);

					if (_isDecaying)
					{
						DecayMeter(Time.deltaTime);
					}
					else if (_decayTimer > 0f)
					{
						_decayTimer -= Time.deltaTime;

						if (_decayTimer <= 0f)
						{
							_isDecaying = true;
						}
					}
				}
			}
			catch (OperationCanceledException)
			{
				// Отменено
			}
		}

		public void AddPointsForHit()
		{
			AddPoints(_pointsPerHit);
		}

		public void AddPointsForKill()
		{
			AddPoints(_pointsPerKill);
		}

		private void AddPoints(float points)
		{
			if (_styleSettings == null)
				return;

			_decayTimer = _decayDelay;
			_isDecaying = false;

			_totalScore += points;

			var currentRank = _styleSettings.GetRank(_currentRankIndex);
			float multiplier = currentRank != null ? currentRank.Multiplier : 1f;
    
			float pointsWithMultiplier = points * multiplier;
			_totalScore += pointsWithMultiplier;
    
			_totalPoints += points;
    
			float pointsForCurrentRank = _currentRankIndex * _pointsToNextRank;
			float pointsInCurrentRank = _totalPoints - pointsForCurrentRank;
    
			// Обновляем шкалу
			_currentMeter = Mathf.Clamp01(pointsInCurrentRank / _pointsToNextRank);
    
			// Проверяем, нужно ли повысить ранг
			if (_currentMeter >= 1f && _currentRankIndex < _styleSettings.GetRankCount() - 1)
			{
				IncreaseRank();
			}
    
			// Публикуем события
			EventBus.Instance.Publish(new StylePointsAddedEvent(points, _totalPoints));
			EventBus.Instance.Publish(new StyleScoreChangedEvent(_totalScore));
			EventBus.Instance.Publish(new StyleMeterChangedEvent(_currentMeter, _currentRankIndex));
		}

		private void IncreaseRank()
		{
			if (_styleSettings == null)
				return;

			_currentRankIndex = Mathf.Min(_currentRankIndex + 1, _styleSettings.GetRankCount() - 1);
			_currentMeter = 0f;

			var rank = _styleSettings.GetRank(_currentRankIndex);
			EventBus.Instance.Publish(new StyleRankChangedEvent(_currentRankIndex, rank.RankName, rank.Multiplier));
		}

		private void DecreaseRank()
		{
			if (_styleSettings == null)
				return;

			if (_currentRankIndex > 0)
			{
				_currentRankIndex--;
				_currentMeter = 1f;

				var rank = _styleSettings.GetRank(_currentRankIndex);
				EventBus.Instance.Publish(new StyleRankChangedEvent(_currentRankIndex, rank.RankName, rank.Multiplier));
			}
		}

		private void DecayMeter(float deltaTime)
		{
			if (_styleSettings == null)
				return;

			_currentMeter -= _decaySpeed * deltaTime;

			if (_currentMeter <= 0f)
			{
				_currentMeter = 0f;

				if (_currentRankIndex > 0)
				{
					DecreaseRank();
					_currentMeter = 1f;
				}
				else
				{
					_isDecaying = false;
				}
			}

			EventBus.Instance.Publish(new StyleMeterChangedEvent(_currentMeter, _currentRankIndex));
		}

		public void ResetStyle()
		{
			_currentMeter = 0f;
			_currentRankIndex = 0;
			_totalPoints = 0f;
			_totalScore = 0f;
			_decayTimer = 0f;
			_isDecaying = false;

			if (_styleSettings != null)
			{
				var rank = _styleSettings.GetRank(_currentRankIndex);
				EventBus.Instance.Publish(new StyleRankChangedEvent(_currentRankIndex, rank.RankName, rank.Multiplier));
				EventBus.Instance.Publish(new StyleMeterChangedEvent(_currentMeter, _currentRankIndex));
				EventBus.Instance.Publish(new StyleScoreChangedEvent(_totalScore));
			}
		}
	}
}