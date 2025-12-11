using Game.Core;
using UnityEngine;

namespace Game.Client
{
	public enum UpgradeType
	{
		Heal,          // Вылечиться на 10 HP
		ClipCapacity,  // +1 к патронам
		Damage,        // +1 к урону
		ShootForce     // Увеличение силы выкидывания
	}

	[System.Serializable]
	public class UpgradeWeight
	{
		public UpgradeType Type;
		[Range(0f, 100f)] public float Weight = 10f; 
	}
	
	[RequireComponent(typeof(Collider))]
	public class UpgradePickup : MonoBehaviour
	{
		[Header("Upgrade Settings")]
		[SerializeField] private UpgradeType _upgradeType;
		[SerializeField] private float _healAmount = 10f;
		[SerializeField] private int _damageUpgrade = 1;
		[SerializeField] private float _shootForceUpgrade = 5f;

		[Header("Visual")]
		[SerializeField] private GameObject _visualObject;
		[SerializeField] private float _rotationSpeed = 90f;
		[SerializeField] private float _bobSpeed = 2f;
		[SerializeField] private float _bobAmount = 0.2f;

		[Header("Pickup Settings")]
		[SerializeField] private LayerMask _playerLayer;

		[Header("References")]
		[SerializeField] private VacuumGun _vacuumGun;
		[SerializeField] private Health _playerHealth;

		private Vector3 _startPosition;
		private bool _isPickedUp = false;
		private Collider _triggerCollider;

		private void Awake()
		{
			_triggerCollider = GetComponent<Collider>();
			if (_triggerCollider != null)
			{
				_triggerCollider.isTrigger = true;
			}

			_startPosition = transform.position;

			if (_vacuumGun == null)
			{
				_vacuumGun = FindFirstObjectByType<VacuumGun>();
			}

			if (_playerHealth == null)
			{
				var playerHealth = FindFirstObjectByType<PlayerHealth>();
				if (playerHealth != null)
				{
					_playerHealth = playerHealth.GetComponent<Health>();
				}
			}
		}

		private void Update()
		{
			if (_isPickedUp)
				return;

			// Вращение
			if (_visualObject != null)
			{
				_visualObject.transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
			}

			var bobOffset = Mathf.Sin(Time.time * _bobSpeed) * _bobAmount;
			transform.position = _startPosition + Vector3.up * bobOffset;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (_isPickedUp)
				return;

			if (IsPlayer(other.gameObject))
			{
				Pickup();
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

		private void Pickup()
		{
			if (_isPickedUp)
				return;

			_isPickedUp = true;
			ApplyUpgrade();
			Destroy(gameObject);
		}

		private void ApplyUpgrade()
		{
			switch (_upgradeType)
			{
				case UpgradeType.Heal:
					if (_playerHealth != null)
					{
						_playerHealth.Heal(_healAmount);
						Debug.Log($"Подобран апгрейд: Вылечился на {_healAmount} HP");
					}
					break;

				case UpgradeType.ClipCapacity:
					if (_vacuumGun != null)
					{
						_vacuumGun.UpgradeClipCapacity();
						Debug.Log("Подобран апгрейд: +1 к патронам");
					}
					break;

				case UpgradeType.Damage:
					if (_vacuumGun != null)
					{
						_vacuumGun.UpgradeDamage(_damageUpgrade);
						Debug.Log($"Подобран апгрейд: +{_damageUpgrade} к урону");
					}
					break;

				case UpgradeType.ShootForce:
					if (_vacuumGun != null)
					{
						_vacuumGun.UpgradeShootForce(_shootForceUpgrade);
						Debug.Log($"Подобран апгрейд: +{_shootForceUpgrade} к силе выкидывания");
					}
					break;
			}
			
			EventBus.Instance.Publish(new PickUpUpgrade(_upgradeType));
		}
	}
}