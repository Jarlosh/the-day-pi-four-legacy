using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client.UI
{
	public class HudScreen: MonoBehaviour
	{
		[field: SerializeField] public Canvas MenuCanvas { get; private set; }
		
		[field: Space]
		[field:SerializeField] 
		private InputActionReference PauseInputAction { get; set; }
		

		private void OnEnable()
		{
			PauseInputAction.action.performed += OnPauseInputPerformed;
		}

		private void OnDisable()
		{
			PauseInputAction.action.performed -= OnPauseInputPerformed;
		}

		private void OnPauseInputPerformed(InputAction.CallbackContext context)
		{
			MenuCanvas.gameObject.SetActive(true);
			gameObject.SetActive(false);
		}
	}
}