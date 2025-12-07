using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

namespace Game.Core
{
	public class CinemachinePov: CinemachineExtension
	{
		[field: SerializeField] private Transform Orientation { get; set; }

		[field: SerializeField] private float SensitivityX { get; set; } = 100f;
		[field: SerializeField] private float SensitivityY { get; set; } = 100f;

		[field: Range(1, 180f)]
		[field: SerializeField] private float LowerLookLimit { get; set; } = 80f;

		[field: Range(1, 180f)]
		[field: SerializeField] private float UpperLookLimit { get; set; } = 90f;

		[field: SerializeField] private InputActionReference _inputAction; 
		
		private const string SensitivityKey = "MouseSensitivity";
		private float _sensitivityMultiplier = 1f;
		private Vector3 _startingRotation;
		private Vector2 _lookInput;
		
		protected override void OnEnable()
		{
			_inputAction.action.performed += OnInputPerformed;
			_inputAction.action.canceled += OnInputCanceled;
			
			LoadSensitivity();
		}

		private void OnDisable()
		{
			_inputAction.action.performed -= OnInputPerformed;
			_inputAction.action.canceled -= OnInputCanceled;
		}
		
		private void LoadSensitivity()
		{
			_sensitivityMultiplier = PlayerPrefs.GetFloat(SensitivityKey, 0.5f);
			
			SensitivityX = _sensitivityMultiplier * 50f;
			SensitivityY = _sensitivityMultiplier * 50f;
		}

		private void OnInputCanceled(InputAction.CallbackContext context)
		{
			_lookInput = Vector2.zero;
		}

		private void OnInputPerformed(InputAction.CallbackContext context)
		{
			_lookInput = context.ReadValue<Vector2>();
		}

		protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
		{
			if (vcam.Follow)
			{
				if (stage == CinemachineCore.Stage.Aim)
				{
					_startingRotation.x += _lookInput.x * SensitivityX * Time.deltaTime;
					_startingRotation.y -= _lookInput.y * SensitivityY * Time.deltaTime;

					_startingRotation.y = Mathf.Clamp(_startingRotation.y, -UpperLookLimit, LowerLookLimit);

					state.RawOrientation = Quaternion.Euler(_startingRotation.y, _startingRotation.x, 0f);

					Orientation.rotation = Quaternion.Euler(0, _startingRotation.x, 0);
				}
			}
		}
	}
}