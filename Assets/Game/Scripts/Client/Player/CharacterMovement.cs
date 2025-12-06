using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client
{
	public class CharacterMovement: ManagedBehaviour
	{
		public enum MovementState
		{
			Freeze,
			Unlimited,
			Walking,
			Sprinting,
			WallRunning,
			Climbing,
			Vaulting,
			Crouching,
			Sliding,
			Air
		}

		private const float MaxSpeedDelta = 4f;
		private const float UnlimitedSpeed = 999f;

		[field: SerializeField] public Transform Orientation { get; private set; }
		[field: SerializeField] public Climbing ClimbingBehaviour { get; private set; }
		[field: SerializeField] public Rigidbody Rigidbody { get; private set; }
		
		[field: Header("Movement")]
		[field: SerializeField] public float WalkSpeed { get; private set; } = 7f;

		[field: SerializeField] public float SprintSpeed { get; private set; } = 10f;

		[field: SerializeField] public float SlideSpeed { get; private set; } = 30f;

		[field: SerializeField] public float WallRunSpeed { get; private set; } = 30f;

		[field: SerializeField] public float ClimbingSpeed { get; private set; } = 3f;
		[field: SerializeField] public float VaultSpeed { get; private set; } = 15f;
		[field: SerializeField] public float AirMinSpeed { get; private set; } = 7f;

		[field: SerializeField] public float SpeedIncreaseMultiplier { get; private set; } = 1.5f;

		[field: SerializeField] public float SlopeIncreaseMultiplier { get; private set; } = 2.5f;

		[field: Space]
		[field: SerializeField] public float GroundDrag { get; private set; } = 5f;

		[field: SerializeField] public MovementState State { get; private set; }

		[field: SerializeField] public bool Sliding { get; set; }

		[field: SerializeField] public bool Crouching { get; set; }

		[field: SerializeField] public bool WallRunning { get; set; }

		[field: SerializeField] public bool Climbing { get; set; }

		[field: SerializeField] public bool Vaulting { get; set; }

		[field: SerializeField] public bool Freeze { get; set; }

		[field: SerializeField] public bool Unlimited { get; set; }

		[field: SerializeField] public bool Restricted { get; set; }

		[field: Header("Jump")]
		[field: SerializeField] private float JumpForce { get; set; } = 6f;

		[field: SerializeField] private float JumpCooldown { get; set; } = 0.25f;

		[field: SerializeField] private float AirMultiplier { get; set; } = 0.4f;

		[field: Header("Crouching")]
		[field: SerializeField] private float CrouchSpeed { get; set; } = 3.5f;

		[field: SerializeField] private float CrouchYScale { get; set; } = 0.5f;

		[field: Header("Key binds")]
		[field: SerializeField] public InputActionReference InputAction { get; private set; }
		[field: SerializeField] public InputActionReference JumpInputAction { get; private set; }
		[field: SerializeField] public InputActionReference CrouchInputAction { get; private set; }
		[field: SerializeField] public InputActionReference SprintInputAction { get; private set; }

		[field: Header("Ground Check")]
		[field: SerializeField] private float PlayerHeight { get; set; } = 1.85f;

		[field: SerializeField] private LayerMask GroundLayer { get; set; }

		[field: Header("Slope Handling")]
		[field: SerializeField] private float MaxSlopeAngle { get; set; } = 40;

		public TextMeshProUGUI TextSpeed;
		public TextMeshProUGUI TextMode;

		private bool _isSprint = false;
        
		private float _horizontalInput;
		private float _verticalInput;

		private float _moveSpeed;
		private float _startYScale;

		private bool _keepMomentum;
		private bool _grounded = true;
		private bool _canJump = true;
		private bool _exitingSlope;

		private float _desiredMoveSpeed;
		private float _lastDesiredMoveSpeed;

		private Vector3 _moveDirection;
		private RaycastHit _slopeHit;

		public bool Grounded => _grounded;

		private void OnValidate()
		{
			_canJump = true;
			_startYScale = transform.localScale.y;
		}

		private void OnEnable()
		{
			InputAction.action.performed += OnInputPerformed;
			InputAction.action.canceled += OnInputCanceled;
			
			JumpInputAction.action.performed += OnJumpInputPerformed;
			
			CrouchInputAction.action.started += OnCrouchInputStarted;
			CrouchInputAction.action.canceled += OnCrouchInputCanceled;
			
			SprintInputAction.action.performed += OnSprintInputPerformed;
			SprintInputAction.action.canceled += OnSprintInputCanceled;
		}
		
		private void OnDisable()
		{
			InputAction.action.performed -= OnInputPerformed;
			InputAction.action.canceled -= OnInputCanceled;

			JumpInputAction.action.performed -= OnJumpInputPerformed;
			
			CrouchInputAction.action.started -= OnCrouchInputStarted;
			CrouchInputAction.action.canceled -= OnCrouchInputCanceled;
			
			SprintInputAction.action.performed -= OnSprintInputPerformed;
			SprintInputAction.action.canceled -= OnSprintInputCanceled;
		}

		private void OnSprintInputPerformed(InputAction.CallbackContext context)
		{
			_isSprint = true;
		}

		private void OnSprintInputCanceled(InputAction.CallbackContext context)
		{
			_isSprint = false;
		}

		private void OnCrouchInputCanceled(InputAction.CallbackContext context)
		{
			StopCrouch();
		}

		private void OnJumpInputPerformed(InputAction.CallbackContext context)
		{
			if (_canJump && _grounded)
			{
				_canJump = false;

				Jump();

				Invoke(nameof(ResetJump), JumpCooldown);
			}
		}

		private void OnCrouchInputStarted(InputAction.CallbackContext context)
		{
			if (_horizontalInput == 0 && _verticalInput == 0)
			{
				StartCrouch();
			}
		}

		private void OnInputPerformed(InputAction.CallbackContext context)
		{
			_verticalInput = context.ReadValue<Vector2>().y;
			_horizontalInput = context.ReadValue<Vector2>().x;
		}
		
		private void OnInputCanceled(InputAction.CallbackContext context)
		{
			_verticalInput = 0;
			_horizontalInput = 0;
		}

		public override void ManagedUpdate()
		{
			HandleGround();

			SpeedControl();
			HandleState();

			DebugText();

			Rigidbody.linearDamping = _grounded ? GroundDrag : 0f;
		}

		public override void ManagedFixedUpdate()
		{
			Move();
		}

		private void HandleGround()
		{
			_grounded = Physics.Raycast(transform.position, Vector3.down, PlayerHeight * 0.5f + 0.3f, GroundLayer);
		}

		private void HandleState()
		{
			if (Freeze)
			{
				State = MovementState.Freeze;
				Rigidbody.linearVelocity = Vector3.zero;
				_desiredMoveSpeed = 0f;
			}
			else if (Unlimited)
			{
				State = MovementState.Unlimited;
				_desiredMoveSpeed = UnlimitedSpeed;
			}
			else if (Vaulting)
			{
				State = MovementState.Vaulting;
				_desiredMoveSpeed = VaultSpeed;
			}
			else if (Climbing)
			{
				State = MovementState.Climbing;
				_desiredMoveSpeed = ClimbingSpeed;
			}
			else if (WallRunning)
			{
				State = MovementState.WallRunning;
				_desiredMoveSpeed = WallRunSpeed;
			}
			else if (Sliding)
			{
				State = MovementState.Sliding;

				if (OnSlope() && Rigidbody.linearVelocity.y < 0.1f)
				{
					_desiredMoveSpeed = SlideSpeed;
					_keepMomentum = true;
				}
				else
				{
					_desiredMoveSpeed = SprintSpeed;
				}
			}
			else if (Crouching)
			{
				State = MovementState.Crouching;
				_desiredMoveSpeed = CrouchSpeed;
			}
			else if (_grounded && _isSprint)
			{
				State = MovementState.Sprinting;
				_desiredMoveSpeed = SprintSpeed;
			}
			else if (_grounded)
			{
				State = MovementState.Walking;
				_desiredMoveSpeed = WalkSpeed;
			}
			else
			{
				State = MovementState.Air;
				if (_moveSpeed < AirMinSpeed)
				{
					_desiredMoveSpeed = AirMinSpeed;
				}
			}

			bool desiredMoveSpeedHasChanged = Math.Abs(_desiredMoveSpeed - _lastDesiredMoveSpeed) > 0.01f;

			if (desiredMoveSpeedHasChanged)
			{
				if (_keepMomentum)
				{
					StopAllCoroutines();
					StartCoroutine(SmoothlyLerpMoveSpeed());
				}
				else
				{
					_moveSpeed = _desiredMoveSpeed;
				}
			}

			_lastDesiredMoveSpeed = _desiredMoveSpeed;

			if (Mathf.Abs(_desiredMoveSpeed - _moveSpeed) < 0.1f)
			{
				_keepMomentum = false;
			}
		}

		private IEnumerator SmoothlyLerpMoveSpeed()
		{
			var time = 0f;
			var difference = Mathf.Abs(_desiredMoveSpeed - _moveSpeed);
			var startValue = _moveSpeed;

			while (time < difference)
			{
				_moveSpeed = Mathf.Lerp(startValue, _desiredMoveSpeed, time / difference);
				if (OnSlope())
				{
					float slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
					float slopeAngleIncrease = 1 + (slopeAngle / 90f);

					time += Time.deltaTime * SpeedIncreaseMultiplier * SlopeIncreaseMultiplier * slopeAngleIncrease;
				}
				else
				{
					time += Time.deltaTime * SpeedIncreaseMultiplier;
				}

				yield return null;
			}

			_moveSpeed = _desiredMoveSpeed;
		}

		private void Move()
		{
			if (ClimbingBehaviour.ExitingWall)
			{
				return;
			}

			if (Restricted || Freeze)
			{
				return;
			}

			_moveDirection = Orientation.forward * _verticalInput + Orientation.right * _horizontalInput;

			if (OnSlope() && !_exitingSlope)
			{
				Rigidbody.AddForce(GetSlopeMoveDirection(_moveDirection) * _moveSpeed * 20f* Rigidbody.mass, ForceMode.Force);

				if (Rigidbody.linearVelocity.y > 0)
				{
					Rigidbody.AddForce(Vector3.down * (80f * Rigidbody.mass), ForceMode.Force);
				}
			}
			else if (_grounded)
			{
				Rigidbody.AddForce(_moveDirection.normalized * _moveSpeed * 10f * Rigidbody.mass, ForceMode.Force);
			}
			else if (!_grounded)
			{
				Rigidbody.AddForce(_moveDirection.normalized * _moveSpeed * 10f * Rigidbody.mass * AirMultiplier, ForceMode.Force);
			}

			if (!WallRunning)
			{
				Rigidbody.useGravity = !OnSlope();
			}
		}

		private void SpeedControl()
		{
			if (OnSlope() && !_exitingSlope)
			{
				if (Rigidbody.linearVelocity.magnitude > _moveSpeed)
				{
					Rigidbody.linearVelocity = Rigidbody.linearVelocity.normalized * _moveSpeed;
				}
			}
			else
			{
				Vector3 velocity = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);

				if (velocity.magnitude > _moveSpeed)
				{
					Vector3 limitedVelocity = velocity.normalized * _moveSpeed;
					Rigidbody.linearVelocity = new Vector3(limitedVelocity.x, Rigidbody.linearVelocity.y, limitedVelocity.z);
				}
			}
		}

		private void StartCrouch()
		{
			transform.localScale = new Vector3(transform.localScale.x, CrouchYScale, transform.localScale.z);
			Rigidbody.AddForce(Vector3.down * (5f * Rigidbody.mass), ForceMode.Impulse);

			Crouching = true;
		}

		private void StopCrouch()
		{
			transform.localScale = new Vector3(transform.localScale.x, _startYScale, transform.localScale.z);

			Crouching = false;
		}

		private void Jump()
		{
			_exitingSlope = true;
			
			EventBus.Instance.Publish(new PlayerJumpedEvent());

			Rigidbody.linearVelocity = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);

			Rigidbody.AddForce(transform.up * JumpForce * Rigidbody.mass, ForceMode.Impulse);
		}

		private void ResetJump()
		{
			_canJump = true;
			_exitingSlope = false;
		}

		public bool OnSlope()
		{
			if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, PlayerHeight * 0.5f + 0.3f))
			{
				float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
				return angle < MaxSlopeAngle && angle != 0;
			}

			return false;
		}

		public Vector3 GetSlopeMoveDirection(Vector3 direction)
		{
			return Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;
		}

		//debug
		private void DebugText()
		{
			Vector3 flatVel = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);

			if (OnSlope())
				TextSpeed.SetText("Speed: " + Round(Rigidbody.linearVelocity.magnitude, 1) + " / " + Round(_moveSpeed, 1));

			else
				TextSpeed.SetText("Speed: " + Round(flatVel.magnitude, 1) + " / " + Round(_moveSpeed, 1));

			TextMode.SetText(State.ToString());
		}

		public static float Round(float value, int digits)
		{
			float mult = Mathf.Pow(10.0f, (float)digits);
			return Mathf.Round(value * mult) / mult;
		}
	}
}
