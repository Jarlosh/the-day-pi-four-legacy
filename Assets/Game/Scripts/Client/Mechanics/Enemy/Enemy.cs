using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Client
{
	public abstract class Enemy: MonoBehaviour
	{
		[Header("References")] [SerializeField]
		private Health _health;

		[Header("Settings")] [SerializeField] private float _maxHealth = 50f;
		
		[Header("Animation References")]
		[SerializeField] private EnemySpawnAnimation _spawnAnimation;
		[SerializeField] private EnemyDespawnAnimation _despawnAnimation;

		private CancellationTokenSource _despawnCts;
		
		public bool IsDead { get; private set; }

		protected virtual void Awake()
		{
			if (_spawnAnimation == null)
			{
				_spawnAnimation = GetComponentInChildren<EnemySpawnAnimation>();
			}
			
			if (_despawnAnimation == null)
			{
				_despawnAnimation = GetComponentInChildren<EnemyDespawnAnimation>();
			}
			
			if (_health == null)
			{
				_health = GetComponent<Health>();
			}

			if (_health != null)
			{
				_health.SetMaxHealth(_maxHealth);
				_health.OnDeath += HandleDeath;
				_health.OnDamageTaken += OnDamaged;
			}
		}
		
		public async UniTaskVoid PlaySpawnAnimation(CancellationToken token)
		{
			if (_spawnAnimation != null)
			{
				_spawnAnimation.PlaySpawnAnimation(token).Forget();
			}
		}

		protected virtual void OnDamaged(float damage, float finalHealth)
		{
		}

		protected virtual void OnDestroy()
		{
			_despawnCts?.Cancel();
			_despawnCts?.Dispose();
			
			if (_health != null)
			{
				_health.OnDeath -= HandleDeath;
				_health.OnDamageTaken -= OnDamaged;
			}
		}
		

		private void HandleDeath()
		{
			IsDead = true;
			
			EventBus.Instance.Publish(new EnemyDeathEvent(this));
			OnDeath();
			
			_despawnCts = new CancellationTokenSource();
			if (_despawnAnimation != null)
			{
				_despawnAnimation.PlayDespawnAnimation(_despawnCts.Token).Forget();
			}
			else
			{
				DestroyAfterDelay(3f).Forget();
			}
		}
		
		private async UniTaskVoid DestroyAfterDelay(float delay)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(delay));
			if (this != null && gameObject != null)
			{
				Destroy(gameObject);
			}
		}

		protected virtual void OnDeath()
		{
			// Переопределяется в наследниках
			Debug.Log($"{gameObject.name} умер!");
		}
	}
}