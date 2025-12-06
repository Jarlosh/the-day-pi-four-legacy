using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client
{
	[RequireComponent(typeof(Rigidbody))]
	public class VacuumedObject: MonoBehaviour
	{
		[Header("Damage Settings")] [SerializeField]
		private float _damage = 10f;

		[SerializeField] private LayerMask _damageableLayers;

		private Rigidbody _rigidbody;
		private Collider[] _colliders;
		private Transform _targetPoint;
		private float _attractionDistance;
		private bool _isVacuumed;
		private bool _canDealDamage;
		private bool _hasReachedTarget;
		private int _playerLayer;
		private CancellationTokenSource _vacuumCts;
		private CancellationTokenSource _collisionReenableCts;

		public bool IsVacuumed => _isVacuumed;
		public bool HasReachedTarget => _hasReachedTarget;

		private void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_colliders = GetComponentsInChildren<Collider>();
			_playerLayer = LayerMask.NameToLayer("Player");
		}

		private void OnDestroy()
		{
			CancelVacuum();
			CancelCollisionReenable();
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!_canDealDamage)
				return;

			if ((_damageableLayers.value & (1 << collision.gameObject.layer)) == 0)
				return;

			var health = collision.gameObject.GetComponent<IDamageable>();
			if (health != null)
			{
				health.TakeDamage(_damage);
				_canDealDamage = false;
				return;
			}
			
			var hitHandler = collision.gameObject.GetComponent<IHitHandler>();
			if (hitHandler != null)
			{
				hitHandler.TakeDamage(_damage);
				_canDealDamage = false;
			}
		}

		public async UniTaskVoid StartVacuum(Transform targetPoint, float attractionDistance, CancellationToken token)
		{
			_targetPoint = targetPoint;
			_attractionDistance = attractionDistance;
			_isVacuumed = true;
			_hasReachedTarget = false;
			_canDealDamage = false;

			_vacuumCts = CancellationTokenSource.CreateLinkedTokenSource(token);

			if (_rigidbody != null)
			{
				_rigidbody.isKinematic = false;
			}

			SetPlayerCollision(false);

			try
			{
				while (!_hasReachedTarget && !_vacuumCts.Token.IsCancellationRequested)
				{
					await UniTask.Yield(_vacuumCts.Token);
				}
			}
			catch (OperationCanceledException)
			{
				if (!_hasReachedTarget)
				{
					CancelVacuum();
				}
			}
		}

		private void FixedUpdate()
		{
			if (_isVacuumed && !_hasReachedTarget && _targetPoint != null && (_vacuumCts == null || !_vacuumCts.Token.IsCancellationRequested))
			{
				var direction = (_targetPoint.position - transform.position).normalized;
				var distance = Vector3.Distance(transform.position, _targetPoint.position);

				if (distance <= _attractionDistance)
				{
					_hasReachedTarget = true;
				}
				else
				{
					_rigidbody.AddForce(direction * _rigidbody.mass * 50f, ForceMode.Force);
				}
			}
		}

		public void CancelVacuum()
		{
			if (!_isVacuumed)
				return;

			_vacuumCts?.Cancel();
			_vacuumCts?.Dispose();
			_vacuumCts = null;

			_isVacuumed = false;
			_hasReachedTarget = false;
			_canDealDamage = false;

			SetPlayerCollision(true);
		}

		public void SuckIntoPoint(Vector3 point)
		{
			_vacuumCts?.Dispose();
			_vacuumCts = null;

			transform.position = point;

			if (_rigidbody != null)
			{
				_rigidbody.isKinematic = true;
				_rigidbody.linearVelocity = Vector3.zero;
				_rigidbody.angularVelocity = Vector3.zero;
			}

			gameObject.SetActive(false);
		}

		public async UniTaskVoid ShootFromPoint(Vector3 point, Vector3 force, float collisionReenableDelay, CancellationToken token)
		{
			gameObject.SetActive(true);

			transform.position = point;

			_isVacuumed = false;
			_hasReachedTarget = false;
			_canDealDamage = true;

			if (_rigidbody != null)
			{
				_rigidbody.isKinematic = false;
				_rigidbody.linearVelocity = Vector3.zero;
				_rigidbody.angularVelocity = Vector3.zero;
				_rigidbody.AddForce(force, ForceMode.Impulse);
			}

			CancelCollisionReenable();
			_collisionReenableCts = CancellationTokenSource.CreateLinkedTokenSource(token);

			try
			{
				await UniTask.Delay(TimeSpan.FromSeconds(collisionReenableDelay), cancellationToken: _collisionReenableCts.Token);
				SetPlayerCollision(true);
			}
			catch (OperationCanceledException)
			{
				// Отменено - коллизии остаются выключенными
			}
			finally
			{
				_collisionReenableCts?.Dispose();
				_collisionReenableCts = null;
			}
		}

		private void CancelCollisionReenable()
		{
			AsyncUtils.TryCancelDisposeNull(ref _collisionReenableCts);
		}

		private void SetPlayerCollision(bool enable)
		{
			if (_playerLayer == -1)
				return;

			var playerCollider = GetPlayerCollider();
			if (playerCollider == null)
				return;

			foreach (var col in _colliders)
			{
				if (col != null)
				{
					Physics.IgnoreCollision(col, playerCollider, !enable);
				}
			}
		}

		private Collider GetPlayerCollider()
		{
			var player = FindFirstObjectByType<PlayerInput>();
			return player != null ? player.GetComponent<Collider>() : null;
		}
	}
}