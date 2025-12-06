using System;
using Game.Core;
using UnityEngine;

namespace Game.Client
{
	public interface IDamageable
	{
		void TakeDamage(float value);
	}
	
	public class Health: MonoBehaviour, IDamageable
	{
		[Header("Health Settings")] 
		[SerializeField] private float _maxHealth = 100f;

		[SerializeField] private bool _canDie = true;

		private float _currentHealth;

		public float MaxHealth => _maxHealth;
		public float CurrentHealth => _currentHealth;
		public float HealthPercent => _maxHealth > 0 ? _currentHealth / _maxHealth : 0f;
		public bool IsDead => _currentHealth <= 0f;

		public event Action<float> OnHealthChanged;
		public event Action<float, float> OnDamageTaken; // damage, newHealth
		public event Action<float> OnDamageDealt; 
		public event Action OnDeath;
		
		private void Awake()
		{
			_currentHealth = _maxHealth;
		}

		public void TakeDamage(float damage)
		{
			if (IsDead || damage <= 0f)
				return;

			_currentHealth = Mathf.Max(0f, _currentHealth - damage);

			OnHealthChanged?.Invoke(_currentHealth);
			OnDamageTaken?.Invoke(damage, _currentHealth);
			OnDamageDealt?.Invoke(damage);

			if (IsDead && _canDie)
			{
				OnDeath?.Invoke();
				HandleDeath();
			}
		}

		public void Heal(float amount)
		{
			if (IsDead || amount <= 0f)
				return;

			_currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
			OnHealthChanged?.Invoke(_currentHealth);
		}

		public void SetMaxHealth(float maxHealth)
		{
			_maxHealth = Mathf.Max(1f, maxHealth);
			if (_currentHealth > _maxHealth)
			{
				_currentHealth = _maxHealth;
			}

			OnHealthChanged?.Invoke(_currentHealth);
		}

		protected virtual void HandleDeath()
		{
			// Переопределяется в наследниках для кастомной логики смерти
		}
	}
}