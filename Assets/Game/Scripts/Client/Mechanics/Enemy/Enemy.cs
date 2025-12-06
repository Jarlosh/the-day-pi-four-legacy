using UnityEngine;

namespace Game.Client
{
	public abstract class Enemy: MonoBehaviour
	{
		[Header("References")] [SerializeField]
		private Health _health;

		[Header("Settings")] [SerializeField] private float _maxHealth = 50f;

		protected bool IsDead { get; private set; }

		protected virtual void Awake()
		{
			if (_health == null)
			{
				_health = GetComponent<Health>();
			}

			if (_health != null)
			{
				_health.SetMaxHealth(_maxHealth);
				_health.OnDeath += HandleDeath;
			}
		}

		protected virtual void OnDestroy()
		{
			if (_health != null)
			{
				_health.OnDeath -= HandleDeath;
			}
		}

		private void HandleDeath()
		{
			IsDead = true;
			
			EventBus.Instance.Publish(new EnemyDeathEvent(this));
			OnDeath();
		}

		protected virtual void OnDeath()
		{
			// Переопределяется в наследниках
			Debug.Log($"{gameObject.name} умер!");
		}
	}
}