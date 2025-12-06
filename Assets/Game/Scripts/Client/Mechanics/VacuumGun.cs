using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client
{
    public class VacuumGun: MonoBehaviour
    {
        public const float MinShootInterval = 0.025f;
        
        [Header("References")]
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _holdPoint; // Точка, куда притягиваются объекты
        [SerializeField] private InputActionReference _vacuumAction; // ПКМ
        [SerializeField] private InputActionReference _shootAction; // ЛКМ
        
        [Header("Vacuum Settings")]
        [SerializeField] private float _vacuumRange = 10f;
        [SerializeField] private float _vacuumRadius = 0.5f; // Радиус рейкаста
        [SerializeField] private int _maxObjects = 7; // Размер "обоймы"
        [SerializeField] private LayerMask _vacuumLayerMask;
        [SerializeField] private float _attractionDistance = 0.1f; // Минимальная дистанция для "всасывания"
        
        [Header("Shoot Settings")]
        [SerializeField] private float _shootForce = 50f;
        [SerializeField] private float _shootInterval = 0.2f; // Интервал между выстрелами
        [SerializeField] private float _collisionReenableDelay = 0.5f; // Задержка перед включением коллизий
        
        private List<VacuumedObject> _vacuumedObjects = new List<VacuumedObject>();
        private List<VacuumedObject> _currentlyVacuuming = new List<VacuumedObject>(); // Объекты, которые сейчас всасываются
        
        private CancellationTokenSource _vacuumCts;
        private CancellationTokenSource _shootCts;
        
        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
                
            _vacuumAction.action.performed += OnVacuumPerformed;
            _vacuumAction.action.canceled += OnVacuumCanceled;
            _shootAction.action.performed += OnShootPerformed;
            _shootAction.action.canceled += OnShootCanceled;
        }
        
        private void OnDestroy()
        {
            _vacuumAction.action.performed -= OnVacuumPerformed;
            _vacuumAction.action.canceled -= OnVacuumCanceled;
            _shootAction.action.performed -= OnShootPerformed;
            _shootAction.action.canceled -= OnShootCanceled;
            
            CancelVacuum();
            CancelShoot();
        }
        
        private void OnVacuumPerformed(InputAction.CallbackContext _)
        {
            StartVacuum().Forget();
        }
        
        private void OnVacuumCanceled(InputAction.CallbackContext _)
        {
            CancelVacuum();
        }
        
        private void OnShootPerformed(InputAction.CallbackContext _)
        {
            StartShoot().Forget();
        }
        
        private void OnShootCanceled(InputAction.CallbackContext _)
        {
            CancelShoot();
        }

        public void UpgradeClipCapacity()
        {
            _maxObjects += 1;
        }
        
        public void UpgradeShootForce(float value)
        {
            _shootForce += value;
        }
        
        public void UpgradeShootInterval(float value)
        {
            _shootInterval = Mathf.Max(_shootInterval - value, MinShootInterval);
        }

        public void UpgradeVacuumRange(float value)
        {
            _vacuumRange += value;
        }
        
        public void UpgradeVacuumRadius(float value)
        {
            _vacuumRadius += value;
        }
        
        private async UniTaskVoid StartVacuum()
        {
            CancelVacuum();
            
            EventBus.Instance.Publish(new VacuumStartedEvent());
            
            _vacuumCts = new CancellationTokenSource();
            var token = _vacuumCts.Token;
            
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (_vacuumedObjects.Count >= _maxObjects)
                    {
                        await UniTask.Yield(token);
                        continue;
                    }
                    
                    TryVacuumObjects(token);
                    await UniTask.Yield(token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Отменяем всасывание для объектов, которые ещё не засосались
                CancelCurrentVacuuming();
            }
        }
        
        private void CancelVacuum()
        {
            AsyncUtils.TryCancelDisposeNull(ref _vacuumCts);
         
            EventBus.Instance.Publish(new VacuumStoppedEvent());
            
            CancelCurrentVacuuming();
        }
        
        private void CancelCurrentVacuuming()
        {
            foreach (var obj in _currentlyVacuuming)
            {
                if (obj != null && !obj.HasReachedTarget)
                {
                    obj.CancelVacuum();
                }
            }
            _currentlyVacuuming.Clear();
        }
        
        private void TryVacuumObjects(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            
            var hits = Physics.SphereCastAll(
                _camera.transform.position,
                _vacuumRadius,
                _camera.transform.forward,
                _vacuumRange,
                _vacuumLayerMask
            );
            
            foreach (var hit in hits)
            {
                if (token.IsCancellationRequested)
                    return;
                    
                if (_vacuumedObjects.Count >= _maxObjects)
                    break;
                    
                var vacuumedObj = hit.collider.GetComponentInParent<VacuumedObject>();
                if (vacuumedObj != null && !vacuumedObj.IsVacuumed && !_vacuumedObjects.Contains(vacuumedObj) && !_currentlyVacuuming.Contains(vacuumedObj))
                {
                    AddVacuumedObject(vacuumedObj, token);
                }
            }
        }
        
        private void AddVacuumedObject(VacuumedObject obj, CancellationToken token)
        {
            _currentlyVacuuming.Add(obj);
            obj.StartVacuum(_holdPoint, _attractionDistance, token).Forget();
        }
        
        private void Update()
        {
            UpdateVacuumedObjects();
        }
        
        private void UpdateVacuumedObjects()
        {
            for (int i = _currentlyVacuuming.Count - 1; i >= 0; i--)
            {
                var obj = _currentlyVacuuming[i];
                if (obj == null)
                {
                    _currentlyVacuuming.RemoveAt(i);
                    continue;
                }
                
                if (obj.HasReachedTarget)
                {
                    obj.SuckIntoPoint(_holdPoint.position);
                    _currentlyVacuuming.RemoveAt(i);
                    _vacuumedObjects.Add(obj);
                    
                    EventBus.Instance.Publish(new VacuumSuccessEvent());
                }
            }
        }
        
        private async UniTaskVoid StartShoot()
        {
            CancelShoot();
            
            _shootCts = new CancellationTokenSource();
            var token = _shootCts.Token;
            
            try
            {
                while (_vacuumedObjects.Count > 0 && !token.IsCancellationRequested)
                {
                    var obj = _vacuumedObjects[0];
                    _vacuumedObjects.RemoveAt(0);
                    
                    if (obj != null)
                    {
                        obj.ShootFromPoint(_holdPoint.position, _camera.transform.forward * _shootForce, _collisionReenableDelay, token).Forget();
                        
                        EventBus.Instance.Publish(new ShootEvent());

                    }
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(_shootInterval), cancellationToken: token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Стрельба отменена
            }
        }

        private void CancelShoot()
        {
            AsyncUtils.TryCancelDisposeNull(ref _shootCts);
        }
    }
}
