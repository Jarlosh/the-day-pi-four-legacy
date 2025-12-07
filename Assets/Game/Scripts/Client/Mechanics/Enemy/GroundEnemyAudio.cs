using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Game.Client
{
	[RequireComponent(typeof(AudioSource))]
	public class GroundEnemyAudio : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private GroundEnemy _groundEnemy;
		[SerializeField] private NavMeshAgent _navAgent;
		[SerializeField] private Health _health;
		[SerializeField] private MeleeAttackBehaviour _attackBehaviour;

		[Header("Audio Sources")]
		[SerializeField] private AudioSource _audioSource;

		[Header("Sound Lists")]
		[SerializeField] private List<AudioClip> _footstepSounds = new List<AudioClip>();
		[SerializeField] private List<AudioClip> _deathSounds = new List<AudioClip>();
		[SerializeField] private List<AudioClip> _attackSounds = new List<AudioClip>();
		[SerializeField] private List<AudioClip> _hurtSounds = new List<AudioClip>();

		[Header("Footstep Settings")]
		[SerializeField] private float _footstepInterval = 0.5f; // Интервал между шагами
		[SerializeField] private float _minSpeedForFootsteps = 0.1f; // Минимальная скорость для шагов

		[Header("Pitch Settings")]
		[SerializeField] private float _pitchRandomRange = 0.2f;
		[SerializeField] private float _basePitch = 1f;

		private float _footstepTimer = 0f;
		private bool _wasMoving = false;
		private bool _hasPlayedDeathSound = false;

		private void Awake()
		{
			if (_audioSource == null)
			{
				_audioSource = GetComponent<AudioSource>();
			}

			if (_groundEnemy == null)
			{
				_groundEnemy = GetComponent<GroundEnemy>();
			}

			if (_navAgent == null)
			{
				_navAgent = GetComponent<NavMeshAgent>();
			}

			if (_health == null)
			{
				_health = GetComponent<Health>();
			}

			if (_attackBehaviour == null)
			{
				_attackBehaviour = GetComponent<MeleeAttackBehaviour>();

				_attackBehaviour.OnAttack += OnAttackHandler;
			}

			if (_audioSource != null)
			{
				_audioSource.playOnAwake = false;
				_audioSource.spatialBlend = 1f; // 3D звук
			}
		}

		private void OnDestroy()
		{
			if (_attackBehaviour != null)
			{
				_attackBehaviour.OnAttack -= OnAttackHandler;
			}
		}
		private void OnEnable()
		{
			if (_health != null)
			{
				_health.OnDeath += OnDeath;
				_health.OnDamageTaken += OnDamaged;
			}
		}

		private void OnDisable()
		{
			if (_health != null)
			{
				_health.OnDeath -= OnDeath;
				_health.OnDamageTaken -= OnDamaged;
			}
		}

		private void Update()
		{
			if (_groundEnemy == null || _groundEnemy.IsDead || _navAgent == null)
				return;

			HandleFootsteps();
		}

		private void HandleFootsteps()
		{
			bool isMoving = _navAgent.velocity.magnitude > _minSpeedForFootsteps && !_navAgent.isStopped;

			if (isMoving)
			{
				if (!_wasMoving)
				{
					_wasMoving = true;
					_footstepTimer = 0f;
				}

				_footstepTimer += Time.deltaTime;

				if (_footstepTimer >= _footstepInterval)
				{
					PlayFootstepSound();

					_footstepTimer = 0f;
				}
			}
			else
			{
				_wasMoving = false;
				_footstepTimer = 0f;
			}
		}

		private void PlayFootstepSound()
		{
			if (_audioSource == null || _footstepSounds == null || _footstepSounds.Count == 0)
				return;

			var clip = _footstepSounds[Random.Range(0, _footstepSounds.Count)];
			if (clip == null)
				return;

			PlaySoundWithRandomPitch(clip);
		}

		private void OnDeath()
		{
			if (_hasPlayedDeathSound)
				return;

			_hasPlayedDeathSound = true;

			if (_audioSource == null || _deathSounds == null || _deathSounds.Count == 0)
				return;

			var clip = _deathSounds[Random.Range(0, _deathSounds.Count)];
			if (clip == null)
				return;

			PlaySoundWithRandomPitch(clip);
		}

		private void OnDamaged(float damage, float finalHealth)
		{
			if (_audioSource == null || _hurtSounds == null || _hurtSounds.Count == 0)
				return;

			var clip = _hurtSounds[Random.Range(0, _hurtSounds.Count)];
			if (clip == null)
				return;

			PlaySoundWithRandomPitch(clip);
		}
		
		private void OnAttackHandler()
		{
			PlayAttackSound();
		}

		public void PlayAttackSound()
		{
			if (_audioSource == null || _attackSounds == null || _attackSounds.Count == 0)
				return;

			var clip = _attackSounds[Random.Range(0, _attackSounds.Count)];
			if (clip == null)
				return;

			PlaySoundWithRandomPitch(clip);
		}

		private void PlaySoundWithRandomPitch(AudioClip clip)
		{
			if (_audioSource == null || clip == null)
				return;

			var randomPitch = _basePitch + Random.Range(-_pitchRandomRange, _pitchRandomRange);
			_audioSource.pitch = randomPitch;
			_audioSource.PlayOneShot(clip);
		}
	}
}