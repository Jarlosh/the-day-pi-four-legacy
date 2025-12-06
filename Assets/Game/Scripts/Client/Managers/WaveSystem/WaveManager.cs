using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Shared;
using Game.Shared.Singletons;
using UnityEngine;

namespace Game.Client
{
	[DefaultExecutionOrder(-100)]
	public class WaveManager: MonoBehaviour
	{
		[Header("Wave Settings")] 
		[SerializeField, ExposedScriptableObject(false)]
		private WaveSettings _waveSettings;

		[SerializeField] private float _countdownDuration = 3f; // Обратный отсчет перед волной
		[SerializeField] private float _betweenWavesDelay = 5f; // Перерыв между волнами

		[Header("Ground Spawn Points")] 
		[SerializeField]
		private List<Transform> _groundSpawnPoints = new List<Transform>();

		[SerializeField] private int _groundSpawnPointsPerWave = 3; // Количество случайных точек спавна для наземных

		[Header("Flying Spawn Points")] 
		[SerializeField]
		private List<Transform> _flyingSpawnPoints = new List<Transform>();

		[SerializeField] private int _flyingSpawnPointsPerWave = 3; // Количество случайных точек спавна для летающих

		[Header("References")] [SerializeField]
		private Health _playerHealth;

		private int _currentWaveIndex = -1;
		private int _currentWaveNumber => _currentWaveIndex + 1;
		private List<Enemy> _activeEnemies = new List<Enemy>();
		private CancellationTokenSource _waveCts;
		private bool _isGameActive;

		public int CurrentWaveNumber => _currentWaveNumber;
		public int TotalWaves => _waveSettings != null ? _waveSettings.Waves.Count : 0;
		public bool IsGameActive => _isGameActive;
		
		public int GetActiveEnemiesCount()
		{
			return _activeEnemies.Count;
		}

		private void Awake()
		{
			ServiceLocator.Register(this);
			
			if (_playerHealth == null)
			{
				var playerHealthComponent = FindFirstObjectByType<PlayerHealth>();
				if (playerHealthComponent != null)
				{
					_playerHealth = playerHealthComponent.GetComponent<Health>();
				}
			}

			if (_playerHealth != null)
			{
				_playerHealth.OnDeath += OnPlayerDeath;
			}
		}
		
		private void Start()
		{
			if (_waveSettings == null || _waveSettings.Waves.Count == 0)
			{
				Debug.LogError("WaveSettings не настроен или нет волн!");
				return;
			}

			StartGame().Forget();
		}

		private void OnDestroy()
		{
			_waveCts?.Cancel();
			_waveCts?.Dispose();

			if (_playerHealth != null)
			{
				_playerHealth.OnDeath -= OnPlayerDeath;
			}
		}

		private void OnPlayerDeath()
		{
			EndGame(false);
		}

		private async UniTaskVoid StartGame()
		{
			_isGameActive = true;
			_waveCts = new CancellationTokenSource();
			var token = _waveCts.Token;

			await UniTask.NextFrame();

			var waves = _waveSettings.Waves;
			try
			{
				for (int i = 0; i < waves.Count; i++)
				{
					if (token.IsCancellationRequested)
						break;

					_currentWaveIndex = i;
					var waveData = waves[i];

					await CountdownBeforeWave(token);

					if (token.IsCancellationRequested)
						break;

					EventBus.Instance.Publish(new WaveStartedEvent(_currentWaveNumber, TotalWaves));
					await StartWave(waveData, token);

					if (token.IsCancellationRequested)
						break;

					await WaitForWaveCompletion(token);

					if (token.IsCancellationRequested)
						break;

					EventBus.Instance.Publish(new WaveCompletedEvent(_currentWaveNumber));

					if (i < waves.Count - 1)
					{
						await CountdownBetweenWave(token);
					}
				}

				if (_isGameActive)
				{
					EndGame(true);
				}
			}
			catch (OperationCanceledException)
			{
				// Игра отменена
			}
		}

		private async UniTask CountdownBeforeWave(CancellationToken token)
		{
			var countdownSeconds = Mathf.CeilToInt(_countdownDuration);
			
			EventBus.Instance.Publish(new GameMusicStateChangedEvent(GameMusicState.StateCalm));
				
			for (var i = countdownSeconds; i > 0; i--)
			{
				EventBus.Instance.Publish(new CountdownEvent(i));
				await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
			}
		}

		private async UniTask CountdownBetweenWave(CancellationToken token)
		{
			var countdownSeconds = Mathf.CeilToInt(_betweenWavesDelay);
			
			EventBus.Instance.Publish(new GameMusicStateChangedEvent(GameMusicState.StateCalm));

			for (var i = countdownSeconds; i > 0; i--)
			{
				EventBus.Instance.Publish(new CountdownEvent(i));
				await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
			}
		}

		private async UniTask StartWave(WaveData waveData, CancellationToken token)
		{
			if (waveData == null || waveData.EnemyPrefabs.Count == 0)
			{
				Debug.LogWarning($"Wave {_currentWaveNumber} has no enemy prefabs!");
				return;
			}

			int enemiesSpawned = 0;
			EventBus.Instance.Publish(new GameMusicStateChangedEvent(GameMusicState.StateBattle));

			while (enemiesSpawned < waveData.MaxEnemiesInWave && !token.IsCancellationRequested)
			{
				int spawnsThisFrame = 0;

				while (spawnsThisFrame < waveData.MaxSpawnsPerFrame &&
						enemiesSpawned < waveData.MaxEnemiesInWave &&
						!token.IsCancellationRequested)
				{
					var enemyPrefab = waveData.EnemyPrefabs[UnityEngine.Random.Range(0, waveData.EnemyPrefabs.Count)];

					bool isFlying = enemyPrefab is FlyingEnemy;
					var spawnPoints = GetRandomSpawnPoints(isFlying);

					if (spawnPoints.Count == 0)
					{
						Debug.LogWarning($"No spawn points available for {(isFlying ? "flying" : "ground")} enemies!");
						spawnsThisFrame++;
						continue;
					}

					var spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];

					var enemy = SpawnEnemy(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
					if (enemy != null)
					{
						_activeEnemies.Add(enemy);
						enemiesSpawned++;
					}

					spawnsThisFrame++;
				}

				if (enemiesSpawned < waveData.MaxEnemiesInWave)
				{
					await UniTask.Delay(TimeSpan.FromSeconds(waveData.SpawnInterval), cancellationToken: token);
				}
			}
		}

		private Enemy SpawnEnemy(Enemy enemyPrefab, Vector3 position, Quaternion rotation)
		{
			if (enemyPrefab == null)
				return null;

			var instance = Instantiate(enemyPrefab.gameObject, position, rotation);
			var enemy = instance.GetComponent<Enemy>();

			if (enemy == null)
			{
				Debug.LogWarning($"Prefab {enemyPrefab.name} doesn't have Enemy component!");
				Destroy(instance);
				return null;
			}

			var health = enemy.GetComponent<Health>();
			if (health != null)
			{
				health.OnDeath += () => OnEnemyDeath(enemy);
			}
			
			EventBus.Instance.Publish(new EnemySpawnedEvent(enemy));

			return enemy;
		}

		private void OnEnemyDeath(Enemy enemy)
		{
			_activeEnemies.Remove(enemy);
		}

		private async UniTask WaitForWaveCompletion(CancellationToken token)
		{
			while (_activeEnemies.Count > 0 && !token.IsCancellationRequested)
			{
				await UniTask.Yield(token);
			}
		}

		private List<Transform> GetRandomSpawnPoints(bool isFlying)
		{
			var spawnPoints = isFlying ? _flyingSpawnPoints : _groundSpawnPoints;
			var pointsPerWave = isFlying ? _flyingSpawnPointsPerWave : _groundSpawnPointsPerWave;

			if (spawnPoints.Count == 0)
				return new List<Transform>();

			var shuffled = spawnPoints.OrderBy(x => UnityEngine.Random.value).ToList();
			return shuffled.Take(Mathf.Min(pointsPerWave, shuffled.Count)).ToList();
		}

		private void EndGame(bool won)
		{
			_isGameActive = false;
			_waveCts?.Cancel();

			if (won)
			{
				EventBus.Instance.Publish(new GameCancelEvent(GameResults.Win));
				Debug.Log("Победа! Все волны пройдены!");
			}
			else
			{
				EventBus.Instance.Publish(new GameCancelEvent(GameResults.Defeat));
				Debug.Log("Поражение! Игрок умер!");
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			foreach (var spawnPoint in _groundSpawnPoints)
			{
				if (spawnPoint != null)
				{
					Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
				}
			}

			Gizmos.color = Color.cyan;
			foreach (var spawnPoint in _flyingSpawnPoints)
			{
				if (spawnPoint != null)
				{
					Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
				}
			}
		}
	}
}