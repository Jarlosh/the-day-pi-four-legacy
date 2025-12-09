using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Game.Client
{
	public class VolumeSelector: MonoBehaviour
	{
		[SerializeField] private AudioMixer _audioMixer;
		[SerializeField] private Slider _volumeSlider;

		[SerializeField] private float _minVolume = -60f;
		[SerializeField] private float _maxVolume = 5f;
		
		[SerializeField][Range(0.1f, 5f)]
		private float _exponent = 3f;
        
		private const string VolumeKey = "MasterVolume";

		private void Start()
		{
			var savedSliderValue = PlayerPrefs.GetFloat(VolumeKey, 0.75f);

			_volumeSlider.value = savedSliderValue;
		}

		public void SetVolume(float sliderValue)
		{
			PlayerPrefs.SetFloat(VolumeKey, sliderValue);
			var range = _maxVolume - _minVolume;
			var exponentialFactor = Mathf.Pow(sliderValue, _exponent);
			var volumeValue = _minVolume + range * exponentialFactor;

			_audioMixer.SetFloat(VolumeKey, volumeValue);
		}
	}
}