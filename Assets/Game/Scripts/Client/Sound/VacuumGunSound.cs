using UnityEngine;

namespace Game.Client
{
	public class VacuumGunAudio: MonoBehaviour
	{
		[Header("References")] 
		[SerializeField] private AudioSource _audioSource;

		[Header("Shoot Sound")] 
		[SerializeField] private float _shootVolume = 1f;
		[SerializeField] private AudioClip _shootClip;
		[SerializeField] private AudioClip _shotgunShootSound; // Выстрел из режима дробовика

		[SerializeField] private AudioClip _emptyMagazineSound; // Пустой магазин при попытке стрелять
		[SerializeField] private AudioClip _fullMagazineSound; // Полный магазин при попытке сосать
		[SerializeField] private AudioClip _modeSwitchSound; // Переключение режима стрельбы

		[Header("Vacuum Start Sound")] [SerializeField]
		private AudioClip _vacuumStartClip;

		[SerializeField] private float _vacuumStartVolume = 1f;

		[Header("Vacuum Loop Sound")] [SerializeField]
		private AudioClip _vacuumLoopClip;

		[SerializeField] private float _vacuumLoopVolume = 1f;

		private bool _vacuumLoopIsPlaying = false;

		[Header("Vacuum Success Sound")] [SerializeField]
		private AudioClip _vacuumSuccessClip;

		[SerializeField] private float _vacuumSuccessVolume = 1f;

		private VacuumGun.ShootMode _currentMode = VacuumGun.ShootMode.Single;

		private void Awake()
		{
			if (_audioSource == null)
			{
				_audioSource = GetComponent<AudioSource>();
			}

			if (_audioSource != null)
			{
				_audioSource.loop = false;
			}
		}

		private void OnEnable()
		{
			EventBus.Instance.Subscribe<VacuumStartedEvent>(OnVacuumStarted);
			EventBus.Instance.Subscribe<VacuumStoppedEvent>(OnVacuumStopped);
			EventBus.Instance.Subscribe<VacuumSuccessEvent>(OnVacuumSuccess);
			EventBus.Instance.Subscribe<ShootModeChangedEvent>(OnShootModeChanged);
			EventBus.Instance.Subscribe<EmptyMagazineEvent>(OnEmptyMagazine);
			EventBus.Instance.Subscribe<FullMagazineEvent>(OnFullMagazine);

			EventBus.Instance.Subscribe<ShootEvent>(OnShoot);
		}

		private void OnDisable()
		{
			EventBus.Instance.Unsubscribe<VacuumStartedEvent>(OnVacuumStarted);
			EventBus.Instance.Unsubscribe<VacuumStoppedEvent>(OnVacuumStopped);
			EventBus.Instance.Unsubscribe<VacuumSuccessEvent>(OnVacuumSuccess);
			EventBus.Instance.Unsubscribe<ShootModeChangedEvent>(OnShootModeChanged);
			EventBus.Instance.Unsubscribe<EmptyMagazineEvent>(OnEmptyMagazine);
			EventBus.Instance.Unsubscribe<FullMagazineEvent>(OnFullMagazine);

			EventBus.Instance.Unsubscribe<ShootEvent>(OnShoot);

			StopVacuumLoop();
		}

		private void OnEmptyMagazine(EmptyMagazineEvent evt)
		{
			if (_audioSource != null && _emptyMagazineSound != null)
			{
				_audioSource.PlayOneShot(_emptyMagazineSound);
			}
		}

		private void OnFullMagazine(FullMagazineEvent evt)
		{
			if (_audioSource != null && _fullMagazineSound != null)
			{
				_audioSource.PlayOneShot(_fullMagazineSound);
			}
		}

		private void OnShootModeChanged(ShootModeChangedEvent evt)
		{
			_currentMode = (VacuumGun.ShootMode)evt.ModeIndex;
			PlayModeSwitchSound();
		}
		
		public void PlayModeSwitchSound()
		{
			if (_audioSource != null && _modeSwitchSound != null)
			{
				_audioSource.PlayOneShot(_modeSwitchSound);
			}
		}

		private void OnVacuumStarted(VacuumStartedEvent _)
		{
			PlayVacuumStartSound();
		}

		private void OnVacuumStopped(VacuumStoppedEvent _)
		{
			StopVacuumSound();
		}

		private void OnVacuumSuccess(VacuumSuccessEvent _)
		{
			PlayVacuumSuccessSound();
		}

		private void OnShoot(ShootEvent _)
		{
			PlayShootSound();
		}

		private void PlayShootSound()
		{
			if (_shootClip != null && _audioSource != null)
			{
				switch (_currentMode)
				{
					case VacuumGun.ShootMode.Single:
						_audioSource.PlayOneShot(_shootClip, _shootVolume);
						break;
					case VacuumGun.ShootMode.Shotgun:
						_audioSource.PlayOneShot(_shotgunShootSound, _shootVolume);
						break;
				}
			}
		}

		private void PlayVacuumStartSound()
		{
			if (_vacuumStartClip != null && _audioSource != null)
			{
				_audioSource.PlayOneShot(_vacuumStartClip, _vacuumStartVolume);
			}

			StartVacuumLoop();
		}

		private void PlayVacuumSuccessSound()
		{
			if (_vacuumSuccessClip != null && _audioSource != null)
			{
				_audioSource.PlayOneShot(_vacuumSuccessClip, _vacuumSuccessVolume);
			}
		}

		private void StopVacuumSound()
		{
			StopVacuumLoop();
		}

		private void StartVacuumLoop()
		{
			if (_vacuumLoopClip == null || _audioSource == null || _vacuumLoopIsPlaying)
				return;

			_audioSource.clip = _vacuumLoopClip;
			_audioSource.loop = true;
			_audioSource.volume = _vacuumLoopVolume;
			_audioSource.Play();
			_vacuumLoopIsPlaying = true;
		}

		private void StopVacuumLoop()
		{
			if (_audioSource == null || !_vacuumLoopIsPlaying)
				return;

			_audioSource.Stop();
			_audioSource.loop = false;
			_vacuumLoopIsPlaying = false;
		}
	}
}