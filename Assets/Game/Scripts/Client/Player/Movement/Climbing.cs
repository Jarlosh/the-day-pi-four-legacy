using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client
{
	public class Climbing: ManagedBehaviour
	{
		//[field: SerializeField] private LedgeGrabbing LedgeGrabbingBehaviour { get; set; }
		[field: SerializeField] private Rigidbody Rigidbody { get; set; }
		[field: SerializeField] private Transform Orientation { get; set; }
		[field: SerializeField] private LayerMask WallLayer { get; set; }

		[field: Header("Climbing")]
		[field: SerializeField]
		private float ClimbSpeed { get; set; } = 10f;

		[field: SerializeField] private float MaxClimbTime { get; set; } = 0.75f;

		[field: Header("Climbing Jump")]
		[field: SerializeField]
		private float JumpUpForce { get; set; } = 14f;

		[field: SerializeField] private float JumpBackForce { get; set; } = 12f;
		[field: SerializeField] private int ClimbJumps { get; set; } = 1;
		[field: SerializeField] private KeyCode JumpKey { get; set; } = KeyCode.Space;

		[field: Header("Exiting")]
		[field: SerializeField]
		private float ExitWallTime { get; set; } = 0.2f;

		[field: Header("Detection")]
		[field: SerializeField]
		private float DetectionLength { get; set; } = 0.7f;

		[field: SerializeField] private float SphereCastRadius { get; set; } = 0.25f;
		[field: SerializeField] private float MaxWallLookAngle { get; set; } = 30f;
		[field: SerializeField] private float MinWallNormalAngleChange { get; set; } = 5f;

		private CharacterMovement _characterMovement;

		private bool _exitingWall;
		private bool _climbing;
		private int _climbJumpsLeft;
		private float _climbTimer;
		private float _exitWallTimer;
		private float _wallLookAngle;

		private RaycastHit _frontWallHit;
		private bool _wallFront;

		private Transform _lastWall;
		private Vector3 _lastWallNormal;

		public bool ExitingWall => _exitingWall;
		public bool IsClimbing => _climbing;

		private void OnValidate()
		{
			_characterMovement = GetComponent<CharacterMovement>();
		}

		public override void ManagedUpdate()
		{
			WallCheck();
			UpdateState();

			if (_climbing && !_exitingWall)
			{
				ProcessClimbingMovement();
			}
		}

		private void UpdateState()
		{
			// Ledge Grabbing
			// if (LedgeGrabbingBehaviour.IsHolding)
			// {
			// 	if (_climbing)
			// 	{
			// 		StopClimbing();
			//
			// 		// everything else gets handled by the HandleState() in the LedgeGrabbing.cs
			// 	}
			// }
			//else if
			if (_wallFront && Input.GetKey(KeyCode.W) && _wallLookAngle < MaxWallLookAngle && !_exitingWall)
			{
				if (!_climbing && _climbTimer > 0)
				{
					StartClimbing();
				}

				if (_climbTimer > 0)
				{
					_climbTimer -= Time.deltaTime;
				}

				if (_climbTimer <= 0)
				{
					StopClimbing();
				}
			}
			else if (_exitingWall)
			{
				if (_climbing)
				{
					StopClimbing();
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
				if (_climbing)
				{
					StopClimbing();
				}
			}

			if (_wallFront && Input.GetKeyDown(JumpKey) && _climbJumpsLeft > 0)
			{
				ClimbJump();
			}
		}

		private void WallCheck()
		{
			_wallFront = Physics.SphereCast(
				transform.position,
				SphereCastRadius,
				Orientation.forward,
				out _frontWallHit,
				DetectionLength,
				WallLayer);

			_wallLookAngle = Vector3.Angle(Orientation.forward, -_frontWallHit.normal);

			bool newWall = _frontWallHit.transform != _lastWall || Mathf.Abs(Vector3.Angle(_lastWallNormal, _frontWallHit.normal)) > MinWallNormalAngleChange;

			if (_characterMovement.Grounded)
			{
				_climbTimer = MaxClimbTime;
				_climbJumpsLeft = ClimbJumps;
			}
		}

		private void StartClimbing()
		{
			_climbing = true;
			_characterMovement.Climbing = true;

			_lastWall = _frontWallHit.transform;
			_lastWallNormal = _frontWallHit.normal;
			//camera fov change
		}

		private void ProcessClimbingMovement()
		{
			Rigidbody.linearVelocity = new Vector3(Rigidbody.linearVelocity.x, ClimbSpeed, Rigidbody.linearVelocity.z);
		}

		private void StopClimbing()
		{
			_climbing = false;
			_characterMovement.Climbing = false;
		}

		private void ClimbJump()
		{
			if (_characterMovement.Grounded)
			{
				return;
			}

			// if (LedgeGrabbingBehaviour.IsHolding || LedgeGrabbingBehaviour.ExitingLedge)
			// {
			// 	return;
			// }

			_exitingWall = true;
			_exitWallTimer = ExitWallTime;

			Vector3 forceToApply = transform.up * JumpUpForce + _frontWallHit.normal * JumpBackForce;

			Rigidbody.linearVelocity = new Vector3(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z);
			Rigidbody.AddForce(forceToApply * Rigidbody.mass, ForceMode.Impulse);

			_climbJumpsLeft--;
		}
	}
}