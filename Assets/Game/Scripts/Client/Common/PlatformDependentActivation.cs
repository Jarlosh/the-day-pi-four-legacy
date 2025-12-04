using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Client
{
	public class PlatformDependentActivation : MonoBehaviour
	{
		[Serializable]
		private enum PlatformType
		{
			PC = 0,
			Web = 1,
			Xbox = 2,
			Playstation = 3,
		}

		[field: SerializeField] private List<PlatformType> Platforms { get; set; }

		private void OnEnable()
		{
			var currentPlatform = PlatformType.PC;

			switch (Application.platform)
			{
				case RuntimePlatform.WebGLPlayer:
					currentPlatform = PlatformType.Web;
					break;
				case RuntimePlatform.PS4:
				case RuntimePlatform.PS5:
					currentPlatform = PlatformType.Playstation;
					break;
				case RuntimePlatform.XboxOne:
					currentPlatform = PlatformType.Xbox;
					break;
			}

			gameObject.SetActive(Platforms.Contains(currentPlatform));
		}
	}
}