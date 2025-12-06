using UnityEngine;

namespace Game.Client
{
	public class PlayerHealth : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private Health _health;
        
		[Header("Settings")]
		[SerializeField] private float _maxHealth = 100f;
        
		private void Awake()
		{
			if (_health == null)
			{
				_health = GetComponent<Health>();
			}
            
			if (_health != null)
			{
				_health.SetMaxHealth(_maxHealth);
				_health.OnDeath += HandleDeath;
				_health.OnDamageTaken += HandleDamageTaken;
			}
		}
        
		private void OnDestroy()
		{
			if (_health != null)
			{
				_health.OnDeath -= HandleDeath;
				_health.OnDamageTaken -= HandleDamageTaken;
			}
		}
        
		private void HandleDamageTaken(float damage, float newHealth)
		{
			Debug.Log($"Игрок получил {damage} урона. Здоровье: {newHealth}/{_health.MaxHealth}");
			// Добавить визуальные эффекты, звуки и т.д.
		}
        
		private void HandleDeath()
		{
			Debug.Log("Игрок умер!");
			// Логика смерти игрока: рестарт уровня, экран смерти и т.д.
		}
	}
}