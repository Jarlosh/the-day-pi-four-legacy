using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client
{
    public class VacuumGun: MonoBehaviour
    {
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
        [SerializeField] private float _attractionSpeed = 10f;
        [SerializeField] private float _attractionDistance = 0.1f; // Минимальная дистанция для "всасывания"
        
        [Header("Shoot Settings")]
        [SerializeField] private float _shootForce = 20f;
        [SerializeField] private float _shootInterval = 0.2f; // Интервал между выстрелами
        [SerializeField] private float _collisionReenableDelay = 0.5f; // Задержка перед включением коллизий
        
        private List<VacuumedObject> _vacuumedObjects = new List<VacuumedObject>();
        private bool _isVacuuming;
        
        private Coroutine _shootCoroutine;
        
        private void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;
                
            _vacuumAction.action.performed += OnVacuumPerformed;
            _vacuumAction.action.canceled += OnVacuumCanceled;
            
            _shootAction.action.performed += OnShootPerformed;
        }
        
        private void OnDestroy()
        {
            _vacuumAction.action.performed -= OnVacuumPerformed;
            _vacuumAction.action.canceled -= OnVacuumCanceled;
            
            _shootAction.action.performed -= OnShootPerformed;
        }
        
        private void Update()
        {
            if (_isVacuuming && _shootCoroutine == null)
            {
                TryVacuumObjects();
            }
            
            UpdateVacuumedObjects();
        }
        
        private void OnVacuumPerformed(InputAction.CallbackContext _)
        {
            _isVacuuming = true;
        }
        
        private void OnVacuumCanceled(InputAction.CallbackContext _)
        {
            _isVacuuming = false;
        }
        
        private void OnShootPerformed(InputAction.CallbackContext _)
        {
            if (_vacuumedObjects.Count > 0 && _shootCoroutine == null)
            {
                _shootCoroutine = StartCoroutine(ShootCoroutine());
            }
        }
        
        private void TryVacuumObjects()
        {
            if (_vacuumedObjects.Count >= _maxObjects)
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
                if (_vacuumedObjects.Count >= _maxObjects)
                    break;
                    
                var vacuumedObj = hit.collider.GetComponent<VacuumedObject>();
                if (vacuumedObj != null && !vacuumedObj.IsVacuumed && !_vacuumedObjects.Contains(vacuumedObj))
                {
                    AddVacuumedObject(vacuumedObj);
                }
            }
        }
        
        private void AddVacuumedObject(VacuumedObject obj)
        {
            _vacuumedObjects.Add(obj);
            obj.StartVacuum(_holdPoint, _attractionDistance);
        }
        
        private void UpdateVacuumedObjects()
        {
            for (int i = _vacuumedObjects.Count - 1; i >= 0; i--)
            {
                var obj = _vacuumedObjects[i];
                if (obj == null)
                {
                    _vacuumedObjects.RemoveAt(i);
                    continue;
                }
                
                if (obj.HasReachedTarget)
                {
                    obj.SuckIntoPoint(_holdPoint.position);
                }
            }
        }
        
        private IEnumerator ShootCoroutine()
        {
            while (_vacuumedObjects.Count > 0)
            {
                var obj = _vacuumedObjects[0];
                _vacuumedObjects.RemoveAt(0);
                
                if (obj != null)
                {
                    obj.ShootFromPoint(_holdPoint.position, _camera.transform.forward * _shootForce, _collisionReenableDelay);
                }
                
                yield return new WaitForSeconds(_shootInterval);
            }
            
            _shootCoroutine = null;
        }
    }
}
