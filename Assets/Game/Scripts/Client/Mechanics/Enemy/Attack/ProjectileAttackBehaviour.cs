using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Client
{
	public class ProjectileAttackBehaviour: MonoBehaviour, IEnemyAttackBehaviour
	{
		[Header("Projectile Settings")] [SerializeField]
		private GameObject _projectilePrefab;

		[SerializeField] private float _projectileSpeed = 10f;
		[SerializeField] private float _projectileDamage = 10f;
		[SerializeField] private float _projectileLifetime = 5f;
		[SerializeField] private Transform _spawnPoint;

		private Enemy _enemy;
		private Transform _target;
		private bool _isAttacking;
		private CancellationTokenSource _attackCts;

		public void Initialize(Enemy enemy, Transform target)
		{
			_enemy = enemy;
			_target = target;

			if (_spawnPoint == null)
			{
				_spawnPoint = transform;
			}
		}

		public void StartAttack()
		{
			if (_isAttacking)
				return;

			_isAttacking = true;
			ShootProjectile().Forget();
		}

		public void StopAttack()
		{
			_isAttacking = false;
			_attackCts?.Cancel();
			_attackCts?.Dispose();
			_attackCts = null;
		}

		public void UpdateAttack()
		{
		}

		private async UniTaskVoid ShootProjectile()
		{
			if (_projectilePrefab == null || _target == null || _enemy == null || _enemy.IsDead)
				return;

			_attackCts = new CancellationTokenSource();
			var token = _attackCts.Token;

			try
			{
				Vector3 direction = (_target.position - _spawnPoint.position).normalized;

				var projectileObj = Instantiate(_projectilePrefab, _spawnPoint.position, Quaternion.LookRotation(direction));
				var projectile = projectileObj.GetComponent<EnemyProjectile>();

				if (projectile != null)
				{
					projectile.Initialize(direction * _projectileSpeed, _projectileDamage, _projectileLifetime, token);
				}
				else
				{
					Debug.LogWarning("Projectile prefab doesn't have EnemyProjectile component!");
					Destroy(projectileObj);
				}
			}
			catch (System.OperationCanceledException)
			{
				// Отменено
			}
		}
	}
}