using UnityEngine;

namespace Game.Client
{
	public class Enemy : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private Health _health;
        
		[Header("Settings")]
		[SerializeField] private float _maxHealth = 50f;
        
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
			}
		}
        
		private void OnDestroy()
		{
			if (_health != null)
			{
				_health.OnDeath -= HandleDeath;
			}
		}
        
		private void HandleDeath()
		{
			// Логика смерти противника
			// Например: анимация смерти, выпадение лута, отключение AI и т.д.
			Debug.Log($"{gameObject.name} dead!");
            
			// Можно отключить компоненты или уничтожить объект
			// Destroy(gameObject, 2f); // Уничтожить через 2 секунды
		}
	}
}