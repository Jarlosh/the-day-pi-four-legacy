using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Game.Client
{
	public class VolumeSelector: MonoBehaviour
	{
		[SerializeField] private AudioMixer _audioMixer;
		[SerializeField] private Slider _volumeSlider;

		private const string VolumeKey = "MasterVolume";

		private void Start()
		{
			var savedSliderValue = PlayerPrefs.GetFloat(VolumeKey, 0.75f);

			_volumeSlider.value = savedSliderValue;
		}

		public void SetVolume(float sliderValue)
		{
			PlayerPrefs.SetFloat(VolumeKey, sliderValue);

			var volumeValue = Mathf.Lerp(-80f, 0, sliderValue);

			_audioMixer.SetFloat(VolumeKey, volumeValue);
		}
	}
}