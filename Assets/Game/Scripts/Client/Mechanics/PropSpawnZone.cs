using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Client
{
	public class PropsSpawnZone: MonoBehaviour
	{
		[Header("Zone Settings")] [SerializeField]
		private float _spawnRadius = 10f; // Радиус зоны спавна (горизонтальный круг)

		[SerializeField] private float _spawnHeight = 0f; // Высота спавна (Y координата)
		[SerializeField] private LayerMask _groundLayer; // Слой для проверки земли (опционально)
		[SerializeField] private bool _useGroundCheck = false; // Использовать проверку земли через рейкаст

		[Header("Props Settings")] [SerializeField]
		private List<GameObject> _propPrefabs = new List<GameObject>(); // Список возможных пропсов

		[SerializeField] private int _maxProps = 10; // Максимальное количество пропсов
		[SerializeField] private float _minDistanceBetweenProps = 1f; // Минимальное расстояние между пропсами (можно сделать 0 для отключения)
		[SerializeField] private bool _checkDistance = true; // Проверять ли расстояние между пропсами

		[Header("Spawn Settings")] [SerializeField]
		private bool _spawnOnStart = true; // Спавнить при старте

		[SerializeField] private float _spawnDelay = 0f; // Задержка перед спавном
		[SerializeField] private int _maxSpawnAttempts = 50; // Максимальное количество попыток найти валидную позицию для одного пропа

		private List<GameObject> _spawnedProps = new List<GameObject>();
		private int _spawnedCount = 0;

		private void Start()
		{
			if (_spawnOnStart)
			{
				if (_spawnDelay > 0f)
				{
					Invoke(nameof(SpawnProps), _spawnDelay);
				}
				else
				{
					StartCoroutine(SpawnPropsCoroutine());
				}
			}
		}

		public void SpawnProps()
		{
			StartCoroutine(SpawnPropsCoroutine());
		}

		private IEnumerator SpawnPropsCoroutine()
		{
			if (_propPrefabs.Count == 0)
			{
				Debug.LogWarning($"PropsSpawnZone '{gameObject.name}' has no prop prefabs!");
				yield break;
			}

			_spawnedCount = 0;
			_spawnedProps.Clear();

			// Спавним пропсы до достижения максимума
			while (_spawnedCount < _maxProps)
			{
				// Выбираем случайный проп
				GameObject propPrefab = _propPrefabs[Random.Range(0, _propPrefabs.Count)];
				if (propPrefab == null)
				{
					yield return null;
					continue;
				}

				// Пытаемся найти валидную позицию
				Vector3 spawnPosition = GetValidSpawnPosition();

				if (spawnPosition != Vector3.zero)
				{
					// Спавним проп
					GameObject prop = Instantiate(propPrefab, spawnPosition, GetRandomRotation());
					_spawnedProps.Add(prop);
					_spawnedCount++;
				}
				else
				{
					// Если не удалось найти позицию после всех попыток, пропускаем этот проп
					Debug.LogWarning($"Не удалось найти валидную позицию для пропа после {_maxSpawnAttempts} попыток. Заспавнено {_spawnedCount}/{_maxProps}");
				}

				// Небольшая задержка между спавнами, чтобы не перегружать кадр
				yield return null;
			}
		}

		private Vector3 GetValidSpawnPosition()
		{
			// Пытаемся найти валидную позицию несколько раз
			for (int attempt = 0; attempt < _maxSpawnAttempts; attempt++)
			{
				Vector3 position = GetRandomSpawnPosition();

				// Если проверка расстояния отключена или позиция валидна - возвращаем её
				if (!_checkDistance || IsPositionValid(position))
				{
					return position;
				}
			}

			// Если не удалось найти валидную позицию - возвращаем случайную без проверки
			return GetRandomSpawnPosition();
		}

		private Vector3 GetRandomSpawnPosition()
		{
			// Генерируем случайную позицию в круге
			Vector2 randomCircle = Random.insideUnitCircle * _spawnRadius;
			Vector3 basePosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

			if (_useGroundCheck)
			{
				// Проверяем землю через рейкаст
				RaycastHit hit;
				var startPos = basePosition;
				basePosition.y = 5; // ceil fix crutch
				if (Physics.Raycast(startPos, Vector3.down, out hit, 20f, _groundLayer))
				{
					return hit.point;
				}
				else
				{
					// Если не нашли землю, возвращаем базовую позицию с высотой
					return new Vector3(basePosition.x, _spawnHeight, basePosition.z);
				}
			}
			else
			{
				// Используем фиксированную высоту
				return new Vector3(basePosition.x, transform.position.y + _spawnHeight, basePosition.z);
			}
		}

		private bool IsPositionValid(Vector3 position)
		{
			// Если проверка отключена - всегда валидна
			if (!_checkDistance || _minDistanceBetweenProps <= 0f)
				return true;

			// Проверяем расстояние до всех уже заспавненных пропсов
			foreach (var prop in _spawnedProps)
			{
				if (prop != null)
				{
					float distance = Vector3.Distance(position, prop.transform.position);
					if (distance < _minDistanceBetweenProps)
					{
						return false;
					}
				}
			}

			return true;
		}

		private Quaternion GetRandomRotation()
		{
			// Случайный поворот вокруг оси Y
			return Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
		}

		public void ClearProps()
		{
			foreach (var prop in _spawnedProps)
			{
				if (prop != null)
				{
					Destroy(prop);
				}
			}

			_spawnedProps.Clear();
			_spawnedCount = 0;
		}

		public void RespawnProps()
		{
			ClearProps();
			SpawnProps();
		}

		private void OnDrawGizmosSelected()
		{
			// Рисуем зону спавна в редакторе
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, _spawnRadius);

			// Рисуем центр
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, 0.5f);
		}
	}
}