using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client
{
     [RequireComponent(typeof(Rigidbody))]
    public class VacuumedObject : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Collider[] _colliders;
        private Transform _targetPoint;
        private float _attractionDistance;
        private bool _isVacuumed;
        private bool _hasReachedTarget;
        private int _playerLayer;
        private Coroutine _collisionReenableCoroutine;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        
        public bool IsVacuumed => _isVacuumed;
        public bool HasReachedTarget => _hasReachedTarget;
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>();
            _playerLayer = LayerMask.NameToLayer("Player");
        }
        
        public void StartVacuum(Transform targetPoint, float attractionDistance)
        {
            _targetPoint = targetPoint;
            _attractionDistance = attractionDistance;
            _isVacuumed = true;
            _hasReachedTarget = false;
            
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
            }
            
            SetPlayerCollision(false);
        }
        
        private void FixedUpdate()
        {
            if (_isVacuumed && !_hasReachedTarget && _targetPoint != null)
            {
                var direction = (_targetPoint.position - transform.position).normalized;
                var distance = Vector3.Distance(transform.position, _targetPoint.position);
                
                if (distance <= _attractionDistance)
                {
                    _hasReachedTarget = true;
                }
                else
                {
                    _rigidbody.AddForce(direction * _rigidbody.mass * 50f, ForceMode.Force);
                }
            }
        }
        
        public void SuckIntoPoint(Vector3 point)
        {
            transform.position = point;
            
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
            
            gameObject.SetActive(false);
        }
        
        public void ShootFromPoint(Vector3 point, Vector3 force, float collisionReenableDelay)
        {
            gameObject.SetActive(true);
            
            transform.position = point;
            
            _isVacuumed = false;
            _hasReachedTarget = false;
            
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                _rigidbody.AddForce(force, ForceMode.Impulse);
            }
            
            if (_collisionReenableCoroutine != null)
            {
                StopCoroutine(_collisionReenableCoroutine);
            }
            _collisionReenableCoroutine = StartCoroutine(ReenableCollisionCoroutine(collisionReenableDelay));
        }
        
        private IEnumerator ReenableCollisionCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            SetPlayerCollision(true);
            _collisionReenableCoroutine = null;
        }
        
        private void SetPlayerCollision(bool enable)
        {
            if (_playerLayer == -1)
                return;
                
            var playerCollider = GetPlayerCollider();
            if (playerCollider == null)
                return;
                
            foreach (var col in _colliders)
            {
                if (col != null)
                {
                    Physics.IgnoreCollision(col, playerCollider, !enable);
                }
            }
        }
        
        private Collider GetPlayerCollider()
        {
            var player = FindFirstObjectByType<PlayerInput>();
            return player != null ? player.GetComponent<Collider>() : null;
        }
    }
}
