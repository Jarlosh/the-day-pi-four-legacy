using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client
{
	public class VacuumGun: MonoBehaviour
	{
		public const float MinShootInterval = 0.025f;

		public enum ShootMode
		{
			Single, // По одному объекту
			Shotgun // Дробовик (несколько объектов одновременно)
		}

		[Header("References")] [SerializeField]
		private Camera _camera;

		[SerializeField] private Transform _holdPoint; // Точка, куда притягиваются объекты
		[SerializeField] private InputActionReference _vacuumAction; // ПКМ
		[SerializeField] private InputActionReference _shootAction; // ЛКМ
		[SerializeField] private InputActionReference _nextModeAction; // Следующий режим
		[SerializeField] private InputActionReference _prevModeAction; // Предыдущий режим

		[Header("Vacuum Settings")] [SerializeField]
		private float _vacuumRange = 10f;

		[SerializeField] private float _vacuumRadius = 0.5f; // Радиус рейкаста
		[SerializeField] private int _maxObjects = 7; // Размер "обоймы"
		[SerializeField] private LayerMask _vacuumLayerMask;
		[SerializeField] private float _attractionDistance = 0.1f; // Минимальная дистанция для "всасывания"

		[Header("Shoot Settings")] [SerializeField]
		private float _shootForce = 50f;

		[SerializeField] private float _shootInterval = 0.2f; // Интервал между выстрелами
		[SerializeField] private float _collisionReenableDelay = 0.5f; // Задержка перед включением коллизий
		[SerializeField] private float _referenceMass = 1;

		[Header("Shoot Mode Settings")] [SerializeField]
		private ShootMode _currentShootMode = ShootMode.Single;

		[SerializeField] private int _shotgunProjectileCount = 3; // Количество объектов при выстреле дробовиком
		[SerializeField] private float _shotgunSpreadAngle = 15f; // Угол разброса для дробовика

		private List<VacuumedObject> _vacuumedObjects = new List<VacuumedObject>();
		private List<VacuumedObject> _currentlyVacuuming = new List<VacuumedObject>(); // Объекты, которые сейчас всасываются

		private bool _isVacuuming;
		private CancellationTokenSource _vacuumCts;
		private CancellationTokenSource _shootCts;

		public bool IsVacuuming => _isVacuuming;
		public int VacuumedObjectsCount => _vacuumedObjects.Count;
		public int MaxObjects => _maxObjects;
		public ShootMode CurrentShootMode => _currentShootMode;

		private void Awake()
		{
			if (_camera == null)
			{
				_camera = Camera.main;
			}

			_vacuumAction.action.performed += OnVacuumPerformed;
			_vacuumAction.action.canceled += OnVacuumCanceled;
			_shootAction.action.performed += OnShootPerformed;
			_shootAction.action.canceled += OnShootCanceled;

			if (_nextModeAction != null)
			{
				_nextModeAction.action.performed += OnNextModePerformed;
			}

			if (_prevModeAction != null)
			{
				_prevModeAction.action.performed += OnPrevModePerformed;
			}
		}

		private void OnDestroy()
		{
			_vacuumAction.action.performed -= OnVacuumPerformed;
			_vacuumAction.action.canceled -= OnVacuumCanceled;
			_shootAction.action.performed -= OnShootPerformed;
			_shootAction.action.canceled -= OnShootCanceled;

			if (_nextModeAction != null)
			{
				_nextModeAction.action.performed -= OnNextModePerformed;
			}

			if (_prevModeAction != null)
			{
				_prevModeAction.action.performed -= OnPrevModePerformed;
			}

			CancelVacuum();
			CancelShoot();
		}

		private void OnNextModePerformed(InputAction.CallbackContext _)
		{
			SwitchToNextMode();
		}

		private void OnPrevModePerformed(InputAction.CallbackContext _)
		{
			SwitchToPreviousMode();
		}

		private void SwitchToNextMode()
		{
			ShootMode nextMode = _currentShootMode == ShootMode.Single ? ShootMode.Shotgun : ShootMode.Single;
			SetShootMode(nextMode);
		}

		private void SwitchToPreviousMode()
		{
			ShootMode prevMode = _currentShootMode == ShootMode.Single ? ShootMode.Shotgun : ShootMode.Single;
			SetShootMode(prevMode);
		}

		private void SetShootMode(ShootMode mode)
		{
			if (_currentShootMode == mode)
				return;

			_currentShootMode = mode;
			string modeName = mode == ShootMode.Single ? "Single" : "Shotgun";
			EventBus.Instance.Publish(new ShootModeChangedEvent((int) mode, modeName));
		}

		private void OnVacuumPerformed(InputAction.CallbackContext _)
		{
			StartVacuum().Forget();
		}

		private void OnVacuumCanceled(InputAction.CallbackContext _)
		{
			CancelVacuum();
		}

		private void OnShootPerformed(InputAction.CallbackContext _)
		{
			StartShoot().Forget();
		}

		private void OnShootCanceled(InputAction.CallbackContext _)
		{
			CancelShoot();
		}

		public void UpgradeClipCapacity()
		{
			_maxObjects += 1;
			PublishVacuumedObjectsChanged();
		}

		public void UpgradeShootForce(float value)
		{
			_shootForce += value;
		}

		public void UpgradeShootInterval(float value)
		{
			_shootInterval = Mathf.Max(_shootInterval - value, MinShootInterval);
		}

		public void UpgradeVacuumRange(float value)
		{
			_vacuumRange += value;
		}

		public void UpgradeVacuumRadius(float value)
		{
			_vacuumRadius += value;
		}

		private void PublishVacuumedObjectsChanged()
		{
			EventBus.Instance.Publish(new VacuumedObjectsChangedEvent(_vacuumedObjects.Count, _maxObjects));
		}

		private async UniTaskVoid StartVacuum()
		{
			CancelVacuum();

			EventBus.Instance.Publish(new VacuumStartedEvent());
			_isVacuuming = true;

			_vacuumCts = new CancellationTokenSource();
			var token = _vacuumCts.Token;

			try
			{
				while (!token.IsCancellationRequested)
				{
					if (_vacuumedObjects.Count >= _maxObjects)
					{
						await UniTask.Yield(token);
						continue;
					}

					TryVacuumObjects(token);
					await UniTask.Yield(token);
				}
			}
			catch (System.OperationCanceledException)
			{
				CancelCurrentVacuuming();
			}
		}

		private void CancelVacuum()
		{
			AsyncUtils.TryCancelDisposeNull(ref _vacuumCts);

			EventBus.Instance.Publish(new VacuumStoppedEvent());

			CancelCurrentVacuuming();
		}

		private void CancelCurrentVacuuming()
		{
			_isVacuuming = false;

			foreach (var obj in _currentlyVacuuming)
			{
				if (obj != null && !obj.HasReachedTarget)
				{
					obj.CancelVacuum();
				}
			}

			_currentlyVacuuming.Clear();
		}

		private void TryVacuumObjects(CancellationToken token)
		{
			if (token.IsCancellationRequested)
				return;

			var hits = Physics.SphereCastAll(
				_camera.transform.position,
				_vacuumRadius,
				_camera.transform.forward,
				_vacuumRange,
				_vacuumLayerMask
			);

			foreach (var hit in hits)
			{
				if (token.IsCancellationRequested)
					return;

				if (_vacuumedObjects.Count >= _maxObjects)
					break;

				var vacuumedObj = hit.collider.GetComponentInParent<VacuumedObject>();
				if (vacuumedObj != null && !vacuumedObj.IsVacuumed && !_vacuumedObjects.Contains(vacuumedObj) && !_currentlyVacuuming.Contains(vacuumedObj))
				{
					AddVacuumedObject(vacuumedObj, token);
				}
			}
		}

		private void AddVacuumedObject(VacuumedObject obj, CancellationToken token)
		{
			_currentlyVacuuming.Add(obj);
			obj.StartVacuum(_holdPoint, _attractionDistance, token).Forget();
		}

		private void Update()
		{
			UpdateVacuumedObjects();
		}

		private void UpdateVacuumedObjects()
		{
			for (int i = _currentlyVacuuming.Count - 1; i >= 0; i--)
			{
				var obj = _currentlyVacuuming[i];
				if (obj == null)
				{
					_currentlyVacuuming.RemoveAt(i);
					continue;
				}

				if (_vacuumedObjects.Count >= _maxObjects)
				{
					if (obj != null && !obj.HasReachedTarget)
					{
						obj.CancelVacuum();
					}

					_currentlyVacuuming.RemoveAt(i);
					continue;
				}

				if (obj.HasReachedTarget)
				{
					obj.SuckIntoPoint(_holdPoint.position);
					_currentlyVacuuming.RemoveAt(i);
					_vacuumedObjects.Add(obj);

					EventBus.Instance.Publish(new VacuumSuccessEvent());
					PublishVacuumedObjectsChanged();
				}
			}
		}

		private async UniTaskVoid StartShoot()
		{
			CancelShoot();

			_shootCts = new CancellationTokenSource();
			var token = _shootCts.Token;

			try
			{
				if (_currentShootMode == ShootMode.Single)
				{
					while (_vacuumedObjects.Count > 0 && !token.IsCancellationRequested)
					{
						var obj = _vacuumedObjects[0];
						_vacuumedObjects.RemoveAt(0);

						if (obj != null)
						{
							ShootObject(obj, _camera.transform.forward, token);
							EventBus.Instance.Publish(new ShootEvent());
							PublishVacuumedObjectsChanged();
						}

						await UniTask.Delay(TimeSpan.FromSeconds(_shootInterval), cancellationToken: token);
					}
				}
				else if (_currentShootMode == ShootMode.Shotgun)
				{
					int objectsToShoot = Mathf.Min(_shotgunProjectileCount, _vacuumedObjects.Count);

					if (objectsToShoot > 0)
					{
						for (int i = 0; i < objectsToShoot; i++)
						{
							if (_vacuumedObjects.Count == 0)
								break;

							var obj = _vacuumedObjects[0];
							_vacuumedObjects.RemoveAt(0);

							if (obj != null)
							{
								Vector3 direction = GetShotgunDirection(i, objectsToShoot);
								ShootObject(obj, direction, token);
							}
						}

						EventBus.Instance.Publish(new ShootEvent());
						PublishVacuumedObjectsChanged();
					}

					await UniTask.Delay(TimeSpan.FromSeconds(_shootInterval), cancellationToken: token);
				}
			}
			catch (System.OperationCanceledException)
			{
				// Стрельба отменена
			}
		}

		private Vector3 GetShotgunDirection(int index, int totalCount)
		{
			float spreadStep = totalCount > 1 ? _shotgunSpreadAngle / (totalCount - 1) : 0f;
			float angle = -_shotgunSpreadAngle / 2f + spreadStep * index;

			Quaternion rotation = Quaternion.AngleAxis(angle, _camera.transform.up);
			return rotation * _camera.transform.forward;
		}

		private void ShootObject(VacuumedObject obj, Vector3 direction, CancellationToken token)
		{
			var rb = obj.GetComponent<Rigidbody>();
			var mass = rb != null ? rb.mass : 1;
			var force = (mass / _referenceMass) * _shootForce;
			obj.ShootFromPoint(_holdPoint.position, direction * force, _collisionReenableDelay, token).Forget();
		}

		private void CancelShoot()
		{
			AsyncUtils.TryCancelDisposeNull(ref _shootCts);
		}
	}
}