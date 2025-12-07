using UnityEngine;

namespace Game.Client
{
	public class WeaponSway: MonoBehaviour
	{
		[Header("References")] [SerializeField]
		private CharacterMovement _characterMovement;

		[SerializeField] private Camera _camera;
		[SerializeField] private VacuumGun _vacuumGun;
		[SerializeField] private Transform _handlerTransform; // Handler transform
		[SerializeField] private Transform _gunTransform; // SM_Gun transform (дочерний Handler)

		[Header("Horizontal Sway Settings")] [SerializeField]
		private float _swayAmount = 0.02f; // Амплитуда покачивания влево-вправо

		[SerializeField] private float _swaySpeed = 2f; // Скорость покачивания
		[SerializeField] private float _swaySmoothness = 5f; // Плавность анимации

		[Header("Vacuum Sway Settings")] [SerializeField]
		private float _vacuumSwayMultiplier = 2f; // Множитель покачивания при сосании

		[SerializeField] private float _vacuumSwaySpeed = 3f; // Скорость покачивания при сосании

		[Header("Shoot Recoil Settings")] [SerializeField]
		private float _shootRecoilAmount = 0.05f; // Амплитуда тряски при выстреле

		[SerializeField] private float _shootRecoilDuration = 0.15f; // Длительность тряски
		[SerializeField] private float _shootRecoilSmoothness = 15f; // Плавность возврата после тряски
		[SerializeField] private Vector2 _shootRecoilRange = new Vector2(0.8f, 1.2f); // Диапазон случайной силы тряски

		[Header("Raycast Settings")] [SerializeField]
		private float _raycastDistance = 100f; // Дальность рейкаста

		[SerializeField] private LayerMask _raycastLayerMask = -1; // Маска слоёв для рейкаста

		[Header("Rotation Settings")] [SerializeField]
		private float _rotationSmoothness = 10f; // Плавность поворота Handler

		private Vector3 _baseHandlerLocalPosition;
		private Vector3 _baseGunLocalRotation;
		private float _swayTime = 0f;
		private float _vacuumSwayTime = 0f;
		private Vector3 _targetLookDirection;

		private Vector3 _shootRecoilOffset = Vector3.zero;
		private float _shootRecoilTime = 0f;
		private bool _isRecoiling = false;

		private void Awake()
		{
			if (_camera == null)
				_camera = Camera.main;

			if (_characterMovement == null)
				_characterMovement = GetComponentInParent<CharacterMovement>();

			if (_vacuumGun == null)
				_vacuumGun = GetComponentInParent<VacuumGun>();

			if (_handlerTransform != null)
			{
				_baseHandlerLocalPosition = _handlerTransform.localPosition;
			}

			if (_gunTransform != null)
			{
				_baseGunLocalRotation = _gunTransform.localEulerAngles;
			}
		}

		private void OnEnable()
		{
			EventBus.Instance.Subscribe<ShootEvent>(OnShoot);
		}

		private void OnDisable()
		{
			EventBus.Instance.Unsubscribe<ShootEvent>(OnShoot);
		}

		private void OnShoot(ShootEvent _)
		{
			TriggerShootRecoil();
		}

		private void TriggerShootRecoil()
		{
			_isRecoiling = true;
			_shootRecoilTime = 0f;

			float randomStrength = Random.Range(_shootRecoilRange.x, _shootRecoilRange.y);
			_shootRecoilOffset = new Vector3(
				Random.Range(-1f, 1f) * _shootRecoilAmount * randomStrength,
				Random.Range(-1f, 1f) * _shootRecoilAmount * randomStrength,
				Random.Range(-1f, 1f) * _shootRecoilAmount * randomStrength * 0.5f // Меньше по Z
			);
		}

		private void Update()
		{
			if (_handlerTransform == null || _camera == null)
				return;

			HandleShootRecoil();

			HandleHorizontalSway();

			HandleRaycastRotation();
		}

		private void HandleShootRecoil()
		{
			if (_isRecoiling)
			{
				_shootRecoilTime += Time.deltaTime;

				if (_shootRecoilTime >= _shootRecoilDuration)
				{
					_isRecoiling = false;
					_shootRecoilOffset = Vector3.zero;
				}
				else
				{
					float t = _shootRecoilTime / _shootRecoilDuration;
					float easeOut = 1f - (1f - t) * (1f - t); 
					Vector3 currentOffset = _shootRecoilOffset * (1f - easeOut);

				}
			}
			else if (_shootRecoilOffset != Vector3.zero)
			{
				_shootRecoilOffset = Vector3.Lerp(
					_shootRecoilOffset,
					Vector3.zero,
					Time.deltaTime * _shootRecoilSmoothness
				);

				if (_shootRecoilOffset.magnitude < 0.001f)
				{
					_shootRecoilOffset = Vector3.zero;
				}
			}
		}

		private void HandleHorizontalSway()
		{
			if (_characterMovement == null)
				return;

			float movementSpeed = _characterMovement.Rigidbody.linearVelocity.magnitude;
			bool isMoving = movementSpeed > 0.5f;
			bool isVacuuming = _vacuumGun != null && _vacuumGun.IsVacuuming;

			float horizontalSway = 0f;

			if (isMoving)
			{
				_swayTime += Time.deltaTime * _swaySpeed;
				float speedMultiplier = Mathf.Clamp01(movementSpeed / _characterMovement.SprintSpeed);
				horizontalSway = Mathf.Sin(_swayTime) * _swayAmount * speedMultiplier;
			}
			else
			{
				_swayTime = Mathf.Lerp(_swayTime, 0f, Time.deltaTime * _swaySmoothness);
				horizontalSway = Mathf.Lerp(horizontalSway, 0f, Time.deltaTime * _swaySmoothness);
			}

			if (isVacuuming)
			{
				_vacuumSwayTime += Time.deltaTime * _vacuumSwaySpeed;
				float vacuumSway = Mathf.Sin(_vacuumSwayTime) * _swayAmount * _vacuumSwayMultiplier;
				horizontalSway += vacuumSway;
			}
			else
			{
				_vacuumSwayTime = Mathf.Lerp(_vacuumSwayTime, 0f, Time.deltaTime * _swaySmoothness);
			}

			Vector3 targetPosition = _baseHandlerLocalPosition;
			targetPosition.x += horizontalSway;
			targetPosition += _shootRecoilOffset;

			_handlerTransform.localPosition = Vector3.Lerp(
				_handlerTransform.localPosition,
				targetPosition,
				Time.deltaTime * _swaySmoothness
			);
		}

		private void HandleRaycastRotation()
		{
			Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
			RaycastHit hit;

			Vector3 targetPoint;
			if (Physics.Raycast(ray, out hit, _raycastDistance, _raycastLayerMask))
			{
				targetPoint = hit.point;
			}
			else
			{
				targetPoint = ray.origin + ray.direction * _raycastDistance;
			}

			Vector3 directionToTarget = (targetPoint - _handlerTransform.position).normalized;

			Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
			_handlerTransform.rotation = Quaternion.Slerp(
				_handlerTransform.rotation,
				targetRotation,
				Time.deltaTime * _rotationSmoothness
			);

			if (_gunTransform != null)
			{
				Vector3 gunRotation = _gunTransform.localEulerAngles;
				gunRotation.x = Mathf.LerpAngle(gunRotation.x, _baseGunLocalRotation.x, Time.deltaTime * _rotationSmoothness);
				gunRotation.z = Mathf.LerpAngle(gunRotation.z, _baseGunLocalRotation.z, Time.deltaTime * _rotationSmoothness);
				_gunTransform.localEulerAngles = gunRotation;
			}
		}

		private void OnDestroy()
		{
			EventBus.Instance.Unsubscribe<ShootEvent>(OnShoot);
		}
	}
}