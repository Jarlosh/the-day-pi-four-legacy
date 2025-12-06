using UnityEngine;

namespace Game.Client
{
	public class EnemyAttackController: MonoBehaviour
	{
		[Header("References")] [SerializeField]
		private Enemy _enemy;

		[SerializeField] private Transform _attackPoint;
		[SerializeField] private MonoBehaviour _attackBehaviorComponent; // MonoBehaviour вместо интерфейса

		[Header("Attack Settings")] [SerializeField]
		private float _attackRange = 15f;

		[SerializeField] private float _attackCooldown = 2f;
		[SerializeField] private bool _canAttackWhileMoving = true;

		[Header("Target")] [SerializeField] private Transform _target;

		private IEnemyAttackBehaviour _attackBehavior;
		private float _lastAttackTime;
		private bool _isAttacking;

		private void Awake()
		{
			if (_enemy == null)
			{
				_enemy = GetComponent<Enemy>();
			}

			if (_attackPoint == null)
			{
				_attackPoint = transform;
			}

			if (_target == null)
			{
				FindPlayer();
			}

			// Автоматически находим поведение атаки
			FindAttackBehavior();
		}

		private void Start()
		{
			InitializeAttackBehavior();
		}

		private void FindAttackBehavior()
		{
			// Если не назначено в инспекторе, ищем автоматически
			if (_attackBehaviorComponent == null)
			{
				_attackBehaviorComponent = GetComponent<IEnemyAttackBehaviour>() as MonoBehaviour;
			}

			// Получаем интерфейс из компонента
			if (_attackBehaviorComponent != null)
			{
				_attackBehavior = _attackBehaviorComponent as IEnemyAttackBehaviour;
			}
		}

		private void InitializeAttackBehavior()
		{
			if (_attackBehavior != null && _target != null)
			{
				_attackBehavior.Initialize(_enemy, _target);
			}
		}

		private void Update()
		{
			if (_enemy == null || _enemy.IsDead || _target == null)
			{
				StopAttack();
				return;
			}

			float distanceToTarget = Vector3.Distance(transform.position, _target.position);

			if (distanceToTarget <= _attackRange)
			{
				if (!_isAttacking && Time.time >= _lastAttackTime + _attackCooldown)
				{
					StartAttack();
				}
			}
			else
			{
				StopAttack();
			}

			if (_isAttacking && _attackBehavior != null)
			{
				_attackBehavior.UpdateAttack();
			}
		}

		private void OnDestroy()
		{
			StopAttack();
		}

		private void FindPlayer()
		{
			var playerController = FindFirstObjectByType<PlayerHealth>();
			if (playerController != null)
			{
				_target = playerController.transform;
				return;
			}

			var player = GameObject.FindGameObjectWithTag("Player");
			if (player != null)
			{
				_target = player.transform;
			}
		}

		private void StartAttack()
		{
			if (_attackBehavior == null || _isAttacking)
				return;

			_isAttacking = true;
			_lastAttackTime = Time.time;
			_attackBehavior.StartAttack();
		}

		private void StopAttack()
		{
			if (!_isAttacking)
				return;

			_isAttacking = false;
			_attackBehavior?.StopAttack();
		}

		public void SetAttackBehavior(IEnemyAttackBehaviour behavior)
		{
			_attackBehavior = behavior;
			_attackBehaviorComponent = behavior as MonoBehaviour;

			if (_attackBehavior != null && _target != null)
			{
				_attackBehavior.Initialize(_enemy, _target);
			}
		}
	}
}