using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Game.Client
{
	[RequireComponent(typeof(NavMeshAgent))]
	public class GroundEnemy : Enemy
	{
		[Header("References")]
		[SerializeField] private Transform _lookTarget; // Игрок
		
		[Header("Movement Settings")]
		[SerializeField] private float _movementSpeed = 3f;
		[SerializeField] private float _rotationSpeed = 5f;
		[SerializeField] private float _stoppingDistance = 2f; // Дистанция остановки от игрока
		[SerializeField] private float _updatePathInterval = 0.5f; // Как часто обновлять путь
		[SerializeField] private EnemyAnimator _animator;
		[SerializeField] private float _stunDuration;

		[SerializeField] private List<Collider> _colliders;
		
		private NavMeshAgent _navAgent;
		private float _lastPathUpdateTime;
		private float _lastStunStart = float.MinValue;

		protected override void Awake()
		{
			base.Awake();
            
			_navAgent = GetComponent<NavMeshAgent>();
            
			if (_navAgent != null)
			{
				_navAgent.speed = _movementSpeed;
				_navAgent.stoppingDistance = _stoppingDistance;
				_navAgent.updateRotation = false; // Управляем поворотом вручную
			}
            
			if (_lookTarget == null)
			{
				FindPlayer();
			}
		}
        
		private void Start()
		{
			if (_navAgent != null && _lookTarget != null)
			{
				_navAgent.SetDestination(_lookTarget.position);
			}
		}
        
		private void Update()
		{
			if (IsDead || _lookTarget == null || _navAgent == null)
				return;
            
			LookAtPlayer();
			UpdateDestination();
			// _animator.Set
		}

		protected override void OnDamaged(float damage, float finalHealth)
		{
			base.OnDamaged(damage, finalHealth);
			if(finalHealth > 0)
			{
				_navAgent.isStopped = true;
				_lastStunStart = Time.time;
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
			if (_navAgent.velocity.sqrMagnitude > 0.1f)
			{
				var direction = _navAgent.velocity.normalized;
				direction.y = 0;
                
				if (direction.sqrMagnitude > 0.01f)
				{
					var targetRotation = Quaternion.LookRotation(direction);
					transform.rotation = Quaternion.Slerp(
						transform.rotation,
						targetRotation,
						_rotationSpeed * Time.deltaTime
					);
				}
			}
			else
			{
				var direction = (_lookTarget.position - transform.position).normalized;
				direction.y = 0;
                
				if (direction.sqrMagnitude > 0.01f)
				{
					var targetRotation = Quaternion.LookRotation(direction);
					transform.rotation = Quaternion.Slerp(
						transform.rotation,
						targetRotation,
						_rotationSpeed * Time.deltaTime
					);
				}
			}
		}
        
		private void UpdateDestination()
		{
			if (_lastStunStart + _stunDuration > Time.time)
			{
				return;
			}
			else
			{
				_navAgent.isStopped = false;
			}

			if (Time.time - _lastPathUpdateTime >= _updatePathInterval)
			{
				if (_navAgent.isOnNavMesh)
				{
					_navAgent.SetDestination(_lookTarget.position);
				}
				_lastPathUpdateTime = Time.time;
			}

			// _animator.IsWalking = !_navAgent.isStopped;
		}
        
		protected override void OnDeath()
		{
			base.OnDeath();

			foreach (var collider in _colliders)
			{
				collider.enabled = false;
			}
			
			if (_navAgent != null)
			{
				_navAgent.isStopped = true;
				_navAgent.enabled = false;
			}
		}
	}
}