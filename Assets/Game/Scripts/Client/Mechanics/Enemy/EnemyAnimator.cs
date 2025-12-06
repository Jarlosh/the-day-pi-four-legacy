using System;
using UnityEngine;

namespace Game.Client
{
    public class EnemyAnimator: MonoBehaviour
    {
        private static readonly int Dead = Animator.StringToHash("IsDead");
        private static readonly int Walking = Animator.StringToHash("IsWalking");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int DeadTrigger = Animator.StringToHash("DeadTrigger");
        private static readonly int Speed = Animator.StringToHash("Speed");
        [SerializeField] private Animator _animator;
        [SerializeField] private Health _health;
        [SerializeField] private AnimationCurve _speedCurve;
        [SerializeField] private float _animCurveLimit;

        private Vector3 _lastPosition;

        public bool IsDead
        {
            get => _animator.GetBool(Dead);
            set => _animator.SetBool(Dead, value);
        }

        public bool IsWalking
        {
            get => _animator.GetBool(Walking);
            set => _animator.SetBool(Walking, value);
        }

        private void Awake()
        {
            _lastPosition = transform.position;
            if (_health != null)
            {
                _health.OnDeath += SetDead;
                _health.OnDamageTaken += SetDead;
            }
        }

        private void SetDead(float damage, float resultHp)
        {
            if (damage > 0 && resultHp > 0)
            {
                SetHit();
            }
        }

        public void SetHit()
        {
            _animator.SetTrigger(Hit);
        }

        private void SetDead()
        {
            IsDead = true;
            _animator.SetTrigger(DeadTrigger);
        }

        public void SetSpeed(float speed)
        {
            _animator.SetFloat(Speed, speed);
        }
        
        private void Update()
        {
            var dp = (transform.position - _lastPosition).magnitude / Time.deltaTime;
            SetSpeed(_speedCurve.Evaluate(Mathf.Min(_animCurveLimit, dp / _animCurveLimit)));
            _lastPosition = transform.position;
        }
        // var speed = _navAgent.isStopped ? 0 : _speedCurve.Evaluate(_navAgent.velocity.magnitude / _animCurveLimit);
        // _animator.SetSpeed(speed);
    }
}