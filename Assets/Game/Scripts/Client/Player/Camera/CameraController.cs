using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Cinemachine;

namespace Game.Client
{
	[RequireComponent(typeof(Camera))]
	public class CameraController: MonoBehaviour
	{
		private enum CameraLookType
		{
			FirstPerson
		}

		[Serializable]
		private class CameraData
		{
			[field: SerializeField] public CameraLookType LookType { get; private set; }
			[field: SerializeField] public CinemachineVirtualCameraBase VirtualCamera { get; private set; }
		}

		[field: SerializeField] private Transform Orientation { get; set; }
		[field: SerializeField] private Transform Player { get; set; }
		[field: SerializeField] private Transform PlayerObject { get; set; }

		[field: Space]
		[field: SerializeField] private float RotationSpeed { get; set; }

		[field: Space]
		[field: Header("Camera Look Type")]
		[field: SerializeField] private CameraLookType CurrentCameraLookType { get; set; } = CameraLookType.FirstPerson;
		[field: SerializeField] private List<CameraData> VirtualCameraData { get; set; } = new();
		
		[field: Header("Combat Style")]
		[field: SerializeField] private Transform CombatLookAt { get; set; }

		[field: Header("Input")]
		[field: SerializeField] private KeyCode ChangeLookType { get; set; } = KeyCode.V;
		[field: SerializeField] private KeyCode ThirdCameraBasic { get; set; } = KeyCode.Alpha1;
		[field: SerializeField] private KeyCode ThirdCameraCombat { get; set; } = KeyCode.Alpha2;
		[field: SerializeField] private KeyCode ThirdCameraTopdown { get; set; } = KeyCode.Alpha3;

		private Camera _camera;
		public CinemachineVirtualCameraBase _currentVirtualCamera;

		private float _horizontalInput;
		private float _verticalInput;
		public CinemachineVirtualCameraBase CurrentVirtualCamera => _currentVirtualCamera;

		private void Awake()
		{
			_camera = GetComponent<Camera>();
		}

		private void Start()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		
		public void DoFov(float endValue, float duration = 0.25f)
		{
			if (CurrentCameraLookType == CameraLookType.FirstPerson)
			{
				if (_currentVirtualCamera is CinemachineFreeLook cinemachineFreeCamera)
				{
					cinemachineFreeCamera.DOFieldOfView(endValue, duration);
				}

				if (_currentVirtualCamera is CinemachineVirtualCamera cinemachineVirtualCamera)
				{
					cinemachineVirtualCamera.DOFieldOfView(endValue, duration);
				}
				
				if (_currentVirtualCamera is CinemachineCamera cinemachineCamera)
				{
					cinemachineCamera.DOFieldOfView(endValue, duration);
				}
			}
		}

		public void DoTilt(float zTilt, float duration = 0.25f)
		{
			if (CurrentCameraLookType == CameraLookType.FirstPerson)
			{
				if (_currentVirtualCamera is CinemachineFreeLook cinemachineFreeCamera)
				{
					cinemachineFreeCamera.DODutch(zTilt, duration);
				}

				if (_currentVirtualCamera is CinemachineVirtualCamera cinemachineVirtualCamera)
				{
					cinemachineVirtualCamera.DODutch(zTilt, duration);
				}
				
				if (_currentVirtualCamera is CinemachineCamera cinemachineCamera)
				{
					cinemachineCamera.DODutch(zTilt, duration);
				}
			}
		}
	}
}