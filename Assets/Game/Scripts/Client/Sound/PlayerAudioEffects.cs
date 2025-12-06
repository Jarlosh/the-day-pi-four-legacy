using System.Collections.Generic;
using UnityEngine;

namespace Game.Client
{
	public class PlayerAudioEffects : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterMovement _characterMovement;
        [SerializeField] private Sliding _sliding;
        [SerializeField] private AudioSource _audioSource;
        
        [Header("Sound Lists")]
        [SerializeField] private List<AudioClip> _slideSounds = new List<AudioClip>();
        [SerializeField] private List<AudioClip> _jumpStartSounds = new List<AudioClip>();
        [SerializeField] private List<AudioClip> _landingSounds = new List<AudioClip>();
        [SerializeField] private List<AudioClip> _footstepSounds = new List<AudioClip>();
        
        [Header("Pitch Settings")]
        [SerializeField] private float _pitchRandomRange = 0.2f; // Диапазон случайного изменения pitch (±0.2)
        [SerializeField] private float _basePitch = 1f; // Базовый pitch
        
        [Header("Footstep Settings")]
        [SerializeField] private float _footstepInterval = 0.5f; // Интервал между шагами
        [SerializeField] private float _minSpeedForFootsteps = 0.1f; // Минимальная скорость для шагов
        
        [Header("Debug")]
        [SerializeField] private bool _debugFootsteps = false;
        
        private bool _wasGrounded = true;
        private bool _wasSliding = false;
        private float _footstepTimer = 0f;
        private Vector3 _lastPosition;
        
        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
            
            if (_characterMovement == null)
            {
                _characterMovement = GetComponent<CharacterMovement>();
            }
            
            if (_sliding == null)
            {
                _sliding = GetComponent<Sliding>();
            }
            
            _lastPosition = transform.position;
        }
        
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<PlayerJumpedEvent>(OnPlayerJumped);
        }
        
        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<PlayerJumpedEvent>(OnPlayerJumped);
        }
        
        private void OnPlayerJumped(PlayerJumpedEvent _)
        {
            PlayJumpStartSound();
        }
        
        private void Update()
        {
            if (_characterMovement == null)
                return;
            
            CheckLanding();
            
            CheckSliding();
            
            HandleFootsteps();
            
            _wasGrounded = _characterMovement.Grounded;
            _lastPosition = transform.position;
        }
        
        private void CheckLanding()
        {
            if (!_wasGrounded && _characterMovement.Grounded)
            {
                PlayLandingSound();
            }
        }
        
        private void CheckSliding()
        {
            bool isSliding = _characterMovement.Sliding;
            
            if (!_wasSliding && isSliding)
            {
                PlaySlideSound();
            }
            
            _wasSliding = isSliding;
        }
        
        private void HandleFootsteps()
        {
            if (!_characterMovement.Grounded && !_characterMovement.WallRunning)
            {
                _footstepTimer = 0f;
                return;
            }
            
            if (_characterMovement.Sliding)
            {
                _footstepTimer = 0f;
                return;
            }
            
            if (_characterMovement.Rigidbody == null)
                return;
            
            Vector3 velocity = _characterMovement.Rigidbody.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            float horizontalSpeed = horizontalVelocity.magnitude;
            
            if (_debugFootsteps)
            {
                Debug.Log($"Footstep Check - Grounded: {_characterMovement.Grounded}, " +
                    $"Sliding: {_characterMovement.Sliding}, " +
                    $"Speed: {horizontalSpeed:F2}, " +
                    $"MinSpeed: {_minSpeedForFootsteps}, " +
                    $"Timer: {_footstepTimer:F2}");
            }
            
            if (horizontalSpeed < _minSpeedForFootsteps)
            {
                _footstepTimer = 0f;
                return;
            }
            
            _footstepTimer += Time.deltaTime;
            
            if (_footstepTimer >= _footstepInterval)
            {
                PlayFootstepSound();
                _footstepTimer = 0f;
            }
        }
        
        public void PlayJumpStartSound()
        {
            PlayRandomSound(_jumpStartSounds, "JumpStart");
        }
        
        private void PlaySlideSound()
        {
            PlayRandomSound(_slideSounds, "Slide");
        }
        
        private void PlayLandingSound()
        {
            PlayRandomSound(_landingSounds, "Landing");
        }
        
        private void PlayFootstepSound()
        {
            PlayRandomSound(_footstepSounds, "Footstep");
        }

        private void PlayRandomSound(List<AudioClip> soundList, string debugName = "")
        {
            if (soundList == null || soundList.Count == 0)
                return;

            if (_audioSource == null)
                return;

            AudioClip clip = soundList[Random.Range(0, soundList.Count)];

            if (clip == null)
            {
                return;
            }

            float randomPitch = _basePitch + Random.Range(-_pitchRandomRange, _pitchRandomRange);
            _audioSource.pitch = randomPitch;

            _audioSource.PlayOneShot(clip);

            _audioSource.pitch = _basePitch;

            if (_debugFootsteps && debugName == "Footstep")
            {
                Debug.Log($"Played footstep: {clip.name}, pitch: {randomPitch:F2}");
            }
        }
    }
}