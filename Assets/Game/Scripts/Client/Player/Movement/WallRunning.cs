using UnityEngine;

namespace Game.Client
{
	public class WallRunning: ManagedBehaviour
	{
		[field: SerializeField] private Rigidbody Rigidbody { get; set; }
		[field: SerializeField] private CameraController CameraController { get; set; }
		[field: SerializeField] private Transform Orientation { get; set; }

		[field: SerializeField] private float FoV { get; set; } = 90;
		[field: SerializeField] private float Tilt { get; set; } = 10f;
		
		[field: Header("Wall Running")]
		[field: SerializeField] private float WallRunForce { get; set; } = 200f;
		[field: SerializeField] private float WallJumpUpForce { get; set; } = 7f;
		[field: SerializeField] private float WallJumpSideForce { get; set; } = 12f;
		[field: SerializeField] private float WallClimbingSpeed { get; set; } = 3f;
		[field: SerializeField] private float MaxWallRunTime { get; set; } = 0.7f;

		[field: Header("Gravity")]
		[field: SerializeField] private bool UseGravity { get; set; }
		[field: SerializeField] private float GravityCounterForce { get; set; } = 27f;

		[field: Header("Input")]
		[field: SerializeField] private KeyCode JumpKey { get; set; } = KeyCode.Space;
		[field: SerializeField] private KeyCode UpwardsRunKey { get; set; } = KeyCode.LeftShift;
		[field: SerializeField] private KeyCode DownwardsRunKey { get; set; } = KeyCode.LeftControl;

		[field: Header("Detection")]
		[field: SerializeField] private float WallCheckDistance { get; set; } = 0.7f;
		[field: SerializeField] private float MinJumpHeight { get; set; } = 2f;
		[field: SerializeField] private float ExitWallTime { get; set; } = 0.2f;
		[field: SerializeField] private LayerMask WallLayer { get; set; }
		[field: SerializeField] private LayerMask GroundLayer { get; set; }

		private float _horizontalInput;
		private float _verticalInput;

		private CharacterMovement _characterMovement;

		private float _wallRunTimer;
		private float _exitWallTimer;

		private RaycastHit _leftWallHit;
		private RaycastHit _rightWallHit;

		private bool _upwardsRunning;
		private bool _downwardsRunning;
		private bool _exitingWall;

		private bool _wallLeft;
		private bool _wallRight;

		private void OnValidate()
		{
			_characterMovement = GetComponent<CharacterMovement>();
		}

		public override void ManagedFixedUpdate()
		{
			if (_characterMovement.WallRunning)
			{
				ProcessWallRunningMovement();
			}
		}

		public override void ManagedUpdate()
		{
			CheckForWall();

			HandleInput();
			UpdateState();
		}


		private void CheckForWall()
		{
			_wallRight = Physics.Raycast(transform.position, Orientation.right, out _rightWallHit, WallCheckDistance, WallLayer);
			_wallLeft = Physics.Raycast(transform.position, -Orientation.right, out _leftWallHit, WallCheckDistance, WallLayer);
		}

		private bool AboveGround()
		{
			return !Physics.Raycast(transform.position, Vector3.down, MinJumpHeight, GroundLayer);
		}

		private void HandleInput()
		{
			_horizontalInput = Input.GetAxisRaw("Horizontal");
			_verticalInput = Input.GetAxisRaw("Vertical");

			_upwardsRunning = Input.GetKey(UpwardsRunKey);
			_downwardsRunning = Input.GetKey(DownwardsRunKey);
		}

		private void UpdateState()
		{
			if (_characterMovement.Freeze)
			{
				return;
			}
			
			if ((_wallLeft || _wallRight) && _verticalInput > 0 && AboveGround() && !_exitingWall)
			{
				if (!_characterMovement.WallRunning)
				{
					StartWallRun();
				}

				if (_wallRunTimer > 0)
				{
					_wallRunTimer -= Time.deltaTime;
				}

				if (_wallRunTimer <= 0 && _characterMovement.WallRunning)
				{
					_exitingWall = true;
					_exitWallTimer = ExitWallTime;
				}

				if (Input.GetKeyDown(JumpKey))
				{
					WallJump();
				}
			}

			else if (_exitingWall)
			{
				if (_characterMovement.WallRunning)
				{
					StopWallRun();
				}

				if (_exitWallTimer > 0)
				{
					_exitWallTimer -= Time.deltaTime;
				}

				if (_exitWallTimer <= 0)
				{
					_exitingWall = false;
				}
			}
			else
			{
				if (_characterMovement.WallRunning)
				{
					StopWallRun();
				}
			}
		}

		private void StartWallRun()
		{
			_characterMovement.WallRunning = true;

			_wallRunTimer = MaxWallRunTime;

			Rigidbody.linearVelocity = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);

			CameraController.DoFov(FoV);

			if (_wallLeft)
			{
				CameraController.DoTilt(-Tilt);
			}

			if (_wallRight)
			{
				CameraController.DoTilt(Tilt);
			}
		}

		private void ProcessWallRunningMovement()
		{
			Rigidbody.useGravity = UseGravity;

			Vector3 wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;
			Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

			if ((Orientation.forward - wallForward).magnitude > (Orientation.forward - (-wallForward)).magnitude)
			{
				wallForward = -wallForward;
			}

			Rigidbody.AddForce(wallForward * WallRunForce  *Rigidbody.mass, ForceMode.Force);

			if (_upwardsRunning)
			{
				Rigidbody.linearVelocity = new Vector3(Rigidbody.linearVelocity.x, WallClimbingSpeed, Rigidbody.linearVelocity.z);
			}

			if (_downwardsRunning)
			{
				Rigidbody.linearVelocity = new Vector3(Rigidbody.linearVelocity.x, -WallClimbingSpeed, Rigidbody.linearVelocity.z);
			}

			if (!(_wallLeft && _horizontalInput > 0) && !(_wallRight && _horizontalInput < 0))
			{
				Rigidbody.AddForce(-wallNormal * 100f * Rigidbody.mass, ForceMode.Force);
			}

			if (UseGravity)
			{
				Rigidbody.AddForce(transform.up * GravityCounterForce  *Rigidbody.mass, ForceMode.Force);
			}
		}

		private void StopWallRun()
		{
			_characterMovement.WallRunning = false;

			CameraController.DoFov(80f);
			CameraController.DoTilt(0);
		}

		private void WallJump()
		{
			// if (LedgeGrabbingBehaviour.IsHolding || LedgeGrabbingBehaviour.ExitingLedge)
			// {
			// 	return;
			// }

			_exitingWall = true;
			_exitWallTimer = ExitWallTime;

			Vector3 wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;
			Vector3 forceToApply = transform.up * WallJumpUpForce + wallNormal * WallJumpSideForce;

			Rigidbody.linearVelocity = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);
			Rigidbody.AddForce(forceToApply * Rigidbody.mass, ForceMode.Impulse);
		}
	}
}