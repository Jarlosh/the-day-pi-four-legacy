using Game.Core;
using UnityEngine;

namespace Game.Client
{
	 public class StyleSystemIntegration : MonoBehaviour
    {
        private StyleSystem _styleSystem;
        
        private void Awake()
        {
            // Получаем StyleSystem из ServiceLocator
            try
            {
                _styleSystem = ServiceLocator.Get<StyleSystem>();
            }
            catch
            {
                Debug.LogWarning("StyleSystem не найден в ServiceLocator. Интеграция будет отключена.");
            }
        }
        
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Instance.Subscribe<EnemyDeathEvent>(OnEnemyDeath);
            EventBus.Instance.Subscribe<VacuumSuccessEvent>(OnVacuumSuccess);
            EventBus.Instance.Subscribe<SlidePerformedEvent>(OnSlidePerformed);
        }
        
        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Instance.Unsubscribe<EnemyDeathEvent>(OnEnemyDeath);
            EventBus.Instance.Unsubscribe<VacuumSuccessEvent>(OnVacuumSuccess);
            EventBus.Instance.Unsubscribe<SlidePerformedEvent>(OnSlidePerformed);
        }

        private void OnEnemySpawned(EnemySpawnedEvent spawnEvent)
        {
            if (_styleSystem == null || spawnEvent.Enemy == null)
            {
                return;
            }
            
            var health = spawnEvent.Enemy.GetComponent<Health>();
            if (health != null)
            {
                health.OnDamageDealt += OnEnemyDamaged;
            }
        }
        
        private void OnEnemyDeath(EnemyDeathEvent deathEvent)
        {
            if (_styleSystem == null || deathEvent.Enemy == null)
                return;
            
            var health = deathEvent.Enemy.GetComponent<Health>();
            if (health != null)
            {
                health.OnDamageDealt -= OnEnemyDamaged;
            }
            
            _styleSystem.AddPointsForKill();
        }
        
        private void OnEnemyDamaged(float damage)
        {
            if (_styleSystem != null)
            {
                _styleSystem.AddPointsForHit();
            }
        }
        
        private void OnSlidePerformed(SlidePerformedEvent ev)
        {
            if (_styleSystem != null)
            {
                _styleSystem.AddPointsForSlide();
            }
        }

        private void OnVacuumSuccess(VacuumSuccessEvent ev)
        {
            if (_styleSystem != null)
            {
                _styleSystem.AddPointsForVacuum();
            }
        }
    }
}