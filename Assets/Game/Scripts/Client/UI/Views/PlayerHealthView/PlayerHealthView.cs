using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client.UI
{
	public class PlayerHealthView : UIViewBase
    {
        [Header("References")]
        [SerializeField] private Health _playerHealth;
        [SerializeField] private Slider _healthProgressBar;
        [SerializeField] private TextMeshProUGUI _healthText;
        
        [Header("Settings")]
        [SerializeField] private string _healthFormat = "{0}/{1}"; // Формат: "100/100"
        
        protected override void Init()
        {
            base.Init();
            
            if (_playerHealth == null)
            {
                var playerHealthComponent = FindFirstObjectByType<PlayerHealth>();
                if (playerHealthComponent != null)
                {
                    _playerHealth = playerHealthComponent.GetComponent<Health>();
                }
            }
            
            if (_playerHealth != null)
            {
                UpdateHealth(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
            }
        }
        
        protected override void Subscribe()
        {
            base.Subscribe();
            
            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged += OnHealthChanged;
            }
        }
        
        protected override void Unsubscribe()
        {
            base.Unsubscribe();
            
            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged -= OnHealthChanged;
            }
        }
        
        private void OnHealthChanged(float currentHealth)
        {
            if (_playerHealth != null)
            {
                UpdateHealth(currentHealth, _playerHealth.MaxHealth);
            }
        }
        
        private void UpdateHealth(float currentHealth, float maxHealth)
        {
            if (_healthProgressBar != null)
            {
                _healthProgressBar.maxValue = maxHealth;
                _healthProgressBar.value = currentHealth;
            }
            
            if (_healthText != null)
            {
                _healthText.text = string.Format(_healthFormat, Mathf.CeilToInt(currentHealth), Mathf.CeilToInt(maxHealth));
            }
        }
    }
}