using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client
{
	[RequireComponent(typeof(Rigidbody))]
	public class FlyingEnemy: Enemy
	{
		[Header("References")] [SerializeField]
		private Transform _lookTarget; // Игрок (можно найти автоматически)

		[Header("Movement Settings")] [SerializeField]
		private float _movementRadius = 10f; // Радиус перемещения от стартовой позиции

		[SerializeField] private float _movementSpeed = 5f; // Скорость перемещения
		[SerializeField] private float _minMovementInterval = 1f; // Минимальный интервал
		[SerializeField] private float _maxMovementInterval = 3f; // Максимальный интервал

		[Header("Look Settings")] [SerializeField]
		private float _lookSpeed = 5f; // Скорость поворота к игроку

		[SerializeField] private bool _lookAtPlayer = true; // Всегда смотреть на игрока

		[Header("Obstacle Check")] [SerializeField]
		private LayerMask _obstacleLayerMask; // Слои препятствий

		[SerializeField] private float _obstacleCheckRadius = 0.5f; // Радиус проверки препятствий
		[SerializeField] private float _obstacleCheckDistance = 1f; // Дистанция проверки впереди

		[Header("Height Settings")] [SerializeField]
		private float _minHeight = 2f; // Минимальная высота

		[SerializeField] private float _maxHeight = 10f; // Максимальная высота
		[SerializeField] private bool _maintainHeight = true; // Поддерживать высоту

		private Rigidbody _rigidbody;
		private Vector3 _startPosition;
		private Vector3 _targetPosition;
		private bool _isMoving;
		private CancellationTokenSource _movementCts;

		protected override void Awake()
		{
			base.Awake();

			_rigidbody = GetComponent<Rigidbody>();

			if (_rigidbody != null)
			{
				_rigidbody.isKinematic = true;
				_rigidbody.useGravity = false;
			}

			_startPosition = transform.position;

			if (_lookTarget == null)
			{
				FindPlayer();
			}
		}

		private void Start()
		{
			StartMovement().Forget();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			_movementCts?.Cancel();
			_movementCts?.Dispose();
		}

		private void Update()
		{
			if (IsDead)
				return;

			if (_lookAtPlayer && _lookTarget != null)
			{
				LookAtPlayer();
			}

			if (_maintainHeight)
			{
				MaintainHeight();
			}

			if (_isMoving)
			{
				MoveTowardsTarget();
			}
		}

		private void FindPlayer()
		{
			var playerController = FindFirstObjectByType<PlayerInput>();
			if (playerController != null)
			{
				_lookTarget = playerController.transform;
				return;
			}

			var player = GameObject.FindGameObjectWithTag("Player");
			if (player != null)
			{
				_lookTarget = player.transform;
			}
		}

		private void LookAtPlayer()
		{
			if (_lookTarget == null)
				return;

			var direction = (_lookTarget.position - transform.position).normalized;

			if (direction.sqrMagnitude > 0.01f)
			{
				var targetRotation = Quaternion.LookRotation(direction);
				transform.rotation = Quaternion.Slerp(
					transform.rotation,
					targetRotation,
					_lookSpeed * Time.deltaTime
				);
			}
		}

		private void MaintainHeight()
		{
			var currentHeight = transform.position.y;
			var targetHeight = Mathf.Clamp(currentHeight, _minHeight, _maxHeight);

			if (Mathf.Abs(currentHeight - targetHeight) > 0.1f)
			{
				var newPosition = transform.position;
				newPosition.y = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * 2f);
				transform.position = newPosition;
			}
		}

		private async UniTaskVoid StartMovement()
		{
			_movementCts = new CancellationTokenSource();
			var token = _movementCts.Token;

			try
			{
				while (!token.IsCancellationRequested)
				{
					var interval = UnityEngine.Random.Range(_minMovementInterval, _maxMovementInterval);
					await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);

					if (token.IsCancellationRequested)
						break;

					await MoveToNewPosition(token);
				}
			}
			catch (OperationCanceledException)
			{
				// Движение отменено
			}
		}

		private async UniTask MoveToNewPosition(CancellationToken token)
		{
			Vector3 newTarget = Vector3.zero;
			int attempts = 0;
			const int maxAttempts = 10;

			while (attempts < maxAttempts && !token.IsCancellationRequested)
			{
				var randomDirection = UnityEngine.Random.insideUnitSphere;
				randomDirection.y = 0; // Только горизонтальное перемещение
				randomDirection.Normalize();

				var randomDistance = UnityEngine.Random.Range(0f, _movementRadius);
				var randomHeight = UnityEngine.Random.Range(_minHeight, _maxHeight);

				newTarget = _startPosition + randomDirection * randomDistance;
				newTarget.y = randomHeight;

				if (CanMoveToPosition(newTarget))
				{
					break;
				}

				attempts++;
			}

			if (attempts >= maxAttempts)
			{
				newTarget = transform.position;
			}

			_targetPosition = newTarget;
			_isMoving = true;

			await MoveTowardsTargetAsync(token);

			_isMoving = false;
		}

		private bool CanMoveToPosition(Vector3 targetPosition)
		{
			var direction = (targetPosition - transform.position).normalized;
			var distance = Vector3.Distance(transform.position, targetPosition);

			if (Physics.SphereCast(
					transform.position,
					_obstacleCheckRadius,
					direction,
					out var hit,
					distance,
					_obstacleLayerMask))
			{
				var distanceToObstacle = hit.distance;
				var distanceToTarget = distance;

				if (distanceToObstacle < distanceToTarget * 0.8f)
				{
					return false;
				}
			}

			if (Physics.CheckSphere(targetPosition, _obstacleCheckRadius, _obstacleLayerMask))
			{
				return false;
			}

			return true;
		}

		private void MoveTowardsTarget()
		{
			var direction = (_targetPosition - transform.position).normalized;
			var distance = Vector3.Distance(transform.position, _targetPosition);

			if (distance < 0.5f)
			{
				_isMoving = false;
				return;
			}

			var moveStep = _movementSpeed * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveStep);
		}

		private async UniTask MoveTowardsTargetAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				var distance = Vector3.Distance(transform.position, _targetPosition);

				if (distance < 0.5f)
				{
					break;
				}

				await UniTask.Yield(token);
			}
		}

		protected override void OnDeath()
		{
			base.OnDeath();

			_movementCts?.Cancel();

			_isMoving = false;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Application.isPlaying ? _startPosition : transform.position, _movementRadius);

			if (Application.isPlaying && _isMoving)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(_targetPosition, 0.5f);
				Gizmos.DrawLine(transform.position, _targetPosition);
			}
		}
	}
}