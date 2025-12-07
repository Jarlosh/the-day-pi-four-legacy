using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Client
{
	public class UpgradeSpawner : MonoBehaviour
	{
		[Header("Spawn Settings")]
		[SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
		[SerializeField] private int _upgradesPerWave = 1; // Количество апгрейдов за волну

		[Header("Upgrade Prefabs")]
		[SerializeField] private GameObject _healUpgradePrefab;
		[SerializeField] private GameObject _clipCapacityUpgradePrefab;
		[SerializeField] private GameObject _damageUpgradePrefab;
		[SerializeField] private GameObject _shootForceUpgradePrefab;

		[Header("Weighted Random")]
		[SerializeField] private List<UpgradeWeight> _upgradeWeights = new List<UpgradeWeight>
		{
			new UpgradeWeight { Type = UpgradeType.Heal, Weight = 30f },
			new UpgradeWeight { Type = UpgradeType.ClipCapacity, Weight = 25f },
			new UpgradeWeight { Type = UpgradeType.Damage, Weight = 25f },
			new UpgradeWeight { Type = UpgradeType.ShootForce, Weight = 20f }
		};

		private List<GameObject> _spawnedUpgrades = new List<GameObject>();

		private void Awake()
		{
			// Подписываемся на события волн
			EventBus.Instance.Subscribe<WaveStartedEvent>(OnWaveStarted);
			EventBus.Instance.Subscribe<WaveCompletedEvent>(OnWaveCompleted);
		}

		private void OnDestroy()
		{
			EventBus.Instance.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
			EventBus.Instance.Unsubscribe<WaveCompletedEvent>(OnWaveCompleted);
		}

		private void OnWaveStarted(WaveStartedEvent evt)
		{
			// Очищаем старые апгрейды
			ClearUpgrades();

			// Спавним новые апгрейды
			SpawnUpgrades();
		}

		private void OnWaveCompleted(WaveCompletedEvent evt)
		{
			// Можно очистить апгрейды после волны, или оставить до следующей
		}

		private void SpawnUpgrades()
		{
			if (_spawnPoints.Count == 0)
			{
				Debug.LogWarning("UpgradeSpawner: Нет точек спавна!");
				return;
			}

			// Выбираем случайные точки спавна
			var availablePoints = _spawnPoints.OrderBy(x => Random.value).Take(_upgradesPerWave).ToList();

			foreach (var spawnPoint in availablePoints)
			{
				if (spawnPoint == null)
					continue;

				// Выбираем тип апгрейда через взвешенный рандом
				var upgradeType = GetRandomUpgradeType();

				// Получаем префаб для этого типа
				var prefab = GetPrefabForType(upgradeType);
				if (prefab == null)
				{
					Debug.LogWarning($"UpgradeSpawner: Нет префаба для типа {upgradeType}!");
					continue;
				}

				// Спавним апгрейд
				var upgradeObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
				_spawnedUpgrades.Add(upgradeObj);
			}
		}

		private GameObject GetPrefabForType(UpgradeType type)
		{
			return type switch
			{
				UpgradeType.Heal => _healUpgradePrefab,
				UpgradeType.ClipCapacity => _clipCapacityUpgradePrefab,
				UpgradeType.Damage => _damageUpgradePrefab,
				UpgradeType.ShootForce => _shootForceUpgradePrefab,
				_ => null
			};
		}

		private UpgradeType GetRandomUpgradeType()
		{
			if (_upgradeWeights.Count == 0)
			{
				// Если веса не настроены, возвращаем случайный тип
				return (UpgradeType)Random.Range(0, System.Enum.GetValues(typeof(UpgradeType)).Length);
			}

			// Вычисляем общий вес
			float totalWeight = _upgradeWeights.Sum(w => w.Weight);

			// Генерируем случайное число от 0 до totalWeight
			float randomValue = Random.Range(0f, totalWeight);

			// Находим соответствующий апгрейд
			float currentWeight = 0f;
			foreach (var upgradeWeight in _upgradeWeights)
			{
				currentWeight += upgradeWeight.Weight;
				if (randomValue <= currentWeight)
				{
					return upgradeWeight.Type;
				}
			}

			// Fallback (не должно произойти)
			return _upgradeWeights[0].Type;
		}

		private void ClearUpgrades()
		{
			foreach (var upgrade in _spawnedUpgrades)
			{
				if (upgrade != null)
				{
					Destroy(upgrade);
				}
			}
			_spawnedUpgrades.Clear();
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			foreach (var spawnPoint in _spawnPoints)
			{
				if (spawnPoint != null)
				{
					Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
				}
			}
		}
	}
}