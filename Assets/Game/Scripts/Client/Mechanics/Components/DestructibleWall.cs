using System;
using UnityEngine;

namespace Game.Client
{
	public class DestructibleWall: MonoBehaviour, IDamageable, IHitHandler
	{
		[Header("Settings")]
		[SerializeField] private float _health = 100f;
		[SerializeField] private float _damagePerHit = 50f;
		[SerializeField] private Fracture _fracture;

		public event Action OnDestroyed;

		private void Awake()
		{
			_fracture.callbackOptions.onCompleted.AddListener(DestroyWall);
		}

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
			Debug.Log("Destroyed wall");
		}
	}
}