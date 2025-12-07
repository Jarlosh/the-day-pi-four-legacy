using UnityEngine;

namespace Game.Client
{
	[RequireComponent(typeof(Collider))]
	public class TutorialTriggerZone : MonoBehaviour
	{
		[SerializeField] private DoorTriggerTutorialStep _tutorialStep;
		[SerializeField] private AudioSource _audioSource;
		[SerializeField] private LayerMask _playerLayer;

		private bool _ready = false;
		
		private void Awake()
		{
			var collider = GetComponent<Collider>();
			if (collider != null)
			{
				collider.isTrigger = true;
			}
		}
		
		private void OnTriggerEnter(Collider other)
		{
			if (_ready)
			{
				return;
			}
			
			if ((_playerLayer.value & (1 << other.gameObject.layer)) != 0)
			{
				if (_tutorialStep != null)
				{
					_tutorialStep.OnPlayerEnteredTrigger();
					_audioSource.Play();
					_ready = true;
				}
			}
		}
	}
}