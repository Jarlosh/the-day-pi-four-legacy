using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using UnityEngine;

namespace Game.Client
{
	[RequireComponent(typeof(Rigidbody))]
	public class EnemyProjectile : MonoBehaviour
	{
		[Header("Settings")]
		[SerializeField] private float _damage = 10f;
		[SerializeField] private LayerMask _playerLayerMask;
		[SerializeField] private float _collisionRadius = 0.5f;
        
		private Rigidbody _rigidbody;
		private Vector3 _velocity;
		private float _lifetime;
		private bool _hasHit;
        
		public void Initialize(Vector3 velocity, float damage, float lifetime, CancellationToken token)
		{
			_velocity = velocity;
			_damage = damage;
			_lifetime = lifetime;
            
			if (_rigidbody == null)
			{
				_rigidbody = GetComponent<Rigidbody>();
			}
            
			if (_rigidbody != null)
			{
				_rigidbody.linearVelocity = _velocity;
			}
            
			DestroyAfterLifetime(token).Forget();
		}
        
		private void FixedUpdate()
		{
			if (_hasHit || _rigidbody == null)
				return;
            
			CheckPlayerCollision();
		}
        
		private void CheckPlayerCollision()
		{
			var hits = Physics.OverlapSphere(transform.position, _collisionRadius, _playerLayerMask);
            
			foreach (var hit in hits)
			{
				var health = hit.GetComponent<IHitHandler>();
				if (health != null && hit.gameObject.layer == LayerManager.Player)
				{
					health.TakeDamage(_damage);
					_hasHit = true;
					Destroy(gameObject);
					return;
				}
			}
		}
        
		private async UniTaskVoid DestroyAfterLifetime(CancellationToken token)
		{
			try
			{
				await UniTask.Delay(System.TimeSpan.FromSeconds(_lifetime), cancellationToken: token);
				if (!_hasHit)
				{
					Destroy(gameObject);
				}
			}
			catch (System.OperationCanceledException)
			{
				// Отменено
			}
		}
        
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, _collisionRadius);
		}
	}
}