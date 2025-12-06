using System;
using UnityEngine;

namespace Game.Client
{
	public class DestructibleWall: MonoBehaviour, IDamageable, IHitHandler
	{
		[Header("Settings")]
		[SerializeField] private float _health = 100f;
		[SerializeField] private float _damagePerHit = 50f;

		public event Action OnDestroyed;
		
		public void TakeDamage(float damage)
		{
			_health -= _damagePerHit;

			if (_health <= 0f)
			{
				DestroyWall();
			}
		}

		private void DestroyWall()
		{
			OnDestroyed?.Invoke();
			gameObject.SetActive(false);
		}
	}
}