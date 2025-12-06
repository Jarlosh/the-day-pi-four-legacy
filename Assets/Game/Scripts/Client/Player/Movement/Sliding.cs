using UnityEngine;

namespace Game.Client
{
	public class Sliding: MonoBehaviour
	{
		[field: SerializeField] private Rigidbody Rigidbody { get; set; }
		[field: SerializeField] private CameraController CameraController { get; set; }

		[field: SerializeField] private Transform Orientation { get; set; }
		[field: SerializeField] private Transform PlayerObj { get; set; }

		[field: SerializeField] private float Fov { get; set; } = 110f;
		[field: SerializeField] private float NormalFov { get; set; } = 80;

		[field: Header("Sliding")]
		[field: SerializeField] private float MaxSlideTime { get; set; } = 0.75f;
		[field: SerializeField] private float SlideForce { get; set; } = 200;
		[field: SerializeField] private float SlideYScale { get; set; } = 0.5f;

		[field: Header("Input")]
		[field: SerializeField] private KeyCode SlideKey { get; set; } = KeyCode.LeftControl;

		private CharacterMovement _characterMovement;

		private float _slideTimer;
		private float _startYScale;

		private float _horizontalInput;
		private float _verticalInput;

		private void Start()
		{
			_characterMovement = GetComponent<CharacterMovement>();

			_startYScale = PlayerObj.localScale.y;
		}

		private void Update()
		{
			HandleInput();
		}

		private void FixedUpdate()
		{
			if (_characterMovement.Sliding)
			{
				ProcessSlidingMovement();
			}
		}

		private void HandleInput()
		{
			_horizontalInput = Input.GetAxisRaw("Horizontal");
			_verticalInput = Input.GetAxisRaw("Vertical");

			if (Input.GetKeyDown(SlideKey) && (_horizontalInput != 0 || _verticalInput != 0))
			{
				StartSlide();
			}

			if (Input.GetKeyUp(SlideKey) && _characterMovement.Sliding)
			{
				StopSlide();
			}
		}

		private void StartSlide()
		{
			CameraController.DoFov(Fov);
			
			_characterMovement.Sliding = true;

			PlayerObj.localScale = new Vector3(PlayerObj.localScale.x, SlideYScale, PlayerObj.localScale.z);
			Rigidbody.AddForce(Vector3.down * (100f * Rigidbody.mass), ForceMode.Impulse);

			_slideTimer = MaxSlideTime;
		}

		private void ProcessSlidingMovement()
		{
			Vector3 inputDirection = Orientation.forward * _verticalInput + Orientation.right * _horizontalInput;

			if (!_characterMovement.OnSlope() || Rigidbody.linearVelocity.y > -0.1f)
			{
				Rigidbody.AddForce(inputDirection.normalized * SlideForce * Rigidbody.mass, ForceMode.Force);

				_slideTimer -= Time.deltaTime;
			}
			else
			{
				Rigidbody.AddForce(_characterMovement.GetSlopeMoveDirection(inputDirection) * SlideForce * Rigidbody.mass, ForceMode.Force);
			}

			if (_slideTimer <= 0)
			{
				StopSlide();
			}
		}

		private void StopSlide()
		{
			_characterMovement.Sliding = false;

			PlayerObj.localScale = new Vector3(PlayerObj.localScale.x, _startYScale, PlayerObj.localScale.z);
			
			CameraController.DoFov(NormalFov);
		}
	}
}