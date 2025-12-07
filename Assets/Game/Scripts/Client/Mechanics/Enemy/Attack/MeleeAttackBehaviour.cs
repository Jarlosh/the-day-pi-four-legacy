using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using UnityEngine;

namespace Game.Client
{
	public class MeleeAttackBehaviour: MonoBehaviour
	{
		[SerializeField] private Health _health;		
		[Header("Attack Settings")] 
		[SerializeField] private float _meleeDamage = 5f;

		[SerializeField] private float _attackInterval = 3f;

		[Header("Target")] [SerializeField] private LayerMask _playerLayer;

		private Collider _triggerCollider;
		private HashSet<GameObject> _playersInZone = new HashSet<GameObject>();
		private float _lastAttackTime;

		private void Awake()
		{
			_triggerCollider = GetComponent<Collider>();
			if (_triggerCollider != null)
			{
				_triggerCollider.isTrigger = true;
			}
		}

		private void OnEnable()
		{
			_health.OnDeath += OnDeathHandler;

		}

		private void OnDisable()
		{
			_health.OnDeath -= OnDeathHandler;
		}

		private void OnDeathHandler()
		{
			_playersInZone.Clear();
			gameObject.SetActive(false);
		}

		private void Update()
		{
			if(_health.IsDead)
			{
				return;
			}
			
			if (_playersInZone.Count > 0 && Time.time >= _lastAttackTime + _attackInterval)
			{
				DealDamageToAllPlayers();
				_lastAttackTime = Time.time;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if(_health.IsDead)
			{
				return;
			}
			
			if (IsPlayer(other.gameObject))
			{
				_playersInZone.Add(other.transform.root.gameObject);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (IsPlayer(other.gameObject))
			{
				_playersInZone.Remove(other.transform.root.gameObject);
			}
		}

		private bool IsPlayer(GameObject obj)
		{
			if (_playerLayer != 0 && ((1 << obj.layer) & _playerLayer) != 0)
			{
				return true;
			}

			if (obj.layer == LayerManager.Player)
			{
				return true;
			}

			if (obj.GetComponent<PlayerHealth>() != null || obj.GetComponent<IDamageable>() != null)
			{
				return true;
			}

			return false;
		}

		private void DealDamageToAllPlayers()
		{
			// Удаляем null объекты из списка
			_playersInZone.RemoveWhere(obj => obj == null);

			foreach (var playerObj in _playersInZone)
			{
				if (playerObj == null)
					continue;

				DealDamage(playerObj);
			}
		}

		private void DealDamage(GameObject playerObj)
		{
			// Пытаемся найти Health компонент через IDamageable
			var damageable = playerObj.GetComponentInChildren<IDamageable>();
			if (damageable != null)
			{
				damageable.TakeDamage(_meleeDamage);
				Debug.Log($"[MeleeAttackZone] Нанесено {_meleeDamage} урона игроку");
				return;
			}

			// Пытаемся найти через IHitHandler
			var hitHandler = playerObj.GetComponentInChildren<IHitHandler>();
			if (hitHandler != null)
			{
				hitHandler.TakeDamage(_meleeDamage);
				Debug.Log($"[MeleeAttackZone] Нанесено {_meleeDamage} урона игроку через IHitHandler");
				return;
			}

			// Пытаемся найти Health напрямую
			var health = playerObj.GetComponentInChildren<Health>();
			if (health != null)
			{
				health.TakeDamage(_meleeDamage);
				Debug.Log($"[MeleeAttackZone] Нанесено {_meleeDamage} урона игроку через Health");
				return;
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (_triggerCollider == null)
				_triggerCollider = GetComponent<Collider>();

			if (_triggerCollider != null && _triggerCollider.isTrigger)
			{
				Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

				if (_triggerCollider is BoxCollider boxCollider)
				{
					Gizmos.matrix = transform.localToWorldMatrix;
					Gizmos.DrawCube(boxCollider.center, boxCollider.size);
				}
				else if (_triggerCollider is SphereCollider sphereCollider)
				{
					Gizmos.DrawSphere(transform.position + sphereCollider.center, sphereCollider.radius);
				}
			}
		}
	}
}