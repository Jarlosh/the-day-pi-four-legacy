using UnityEngine;
using UnityEngine.UI;

namespace Game.Client
{
	public class SensitivitySelector : MonoBehaviour
	{
		[SerializeField] private Slider _sensitivitySlider;

		private const string SensitivityKey = "MouseSensitivity";

		private void Start()
		{
			var savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 0.5f);

			if (_sensitivitySlider != null)
			{
				_sensitivitySlider.value = savedSensitivity;
			}
		}

		public void SetSensitivity(float sliderValue)
		{
			PlayerPrefs.SetFloat(SensitivityKey, sliderValue);
			PlayerPrefs.Save();
		}
	}
}