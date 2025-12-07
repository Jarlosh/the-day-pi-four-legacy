using System.Collections.Generic;
using UnityEngine;

namespace Game.Client
{
	public class PlayerHealth : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private Health _health;
        
		[Header("Settings")]
		[SerializeField] private float _maxHealth = 100f;
		[Space]
		[SerializeField] private AudioSource _audioSource;
		[SerializeField] private List<AudioClip> _hitSounds = new List<AudioClip>();
		
		[Header("Pitch Settings")]
		[SerializeField] private float _pitchRandomRange = 0.2f; // Диапазон случайного изменения pitch (±0.2)
		[SerializeField] private float _basePitch = 1f;

		private void Awake()
		{
			if (_health == null)
			{
				_health = GetComponent<Health>();
			}
            
			if (_health != null)
			{
				_health.SetMaxHealth(_maxHealth);
				_health.OnDeath += HandleDeath;
				_health.OnDamageTaken += HandleDamageTaken;
			}
		}
        
		private void OnDestroy()
		{
			if (_health != null)
			{
				_health.OnDeath -= HandleDeath;
				_health.OnDamageTaken -= HandleDamageTaken;
			}
		}
        
		private void HandleDamageTaken(float damage, float newHealth)
		{
			PlayRandomSound(_hitSounds);
			
			Debug.Log($"Игрок получил {damage} урона. Здоровье: {newHealth}/{_health.MaxHealth}");
			// Добавить визуальные эффекты, звуки и т.д.
		}
        
		private void HandleDeath()
		{
			Debug.Log("Игрок умер!");
			// Логика смерти игрока: рестарт уровня, экран смерти и т.д.
		}
		
		private void PlayRandomSound(List<AudioClip> soundList, string debugName = "")
		{
			if (soundList == null || soundList.Count == 0)
				return;

			if (_audioSource == null)
				return;

			var clip = soundList[Random.Range(0, soundList.Count)];

			if (clip == null)
			{
				return;
			}

			var randomPitch = _basePitch + Random.Range(-_pitchRandomRange, _pitchRandomRange);
			_audioSource.pitch = randomPitch;

			_audioSource.PlayOneShot(clip);
			_audioSource.pitch = _basePitch;
		}
	}
}