using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Client
{
	public class EnemyDespawnAnimation : MonoBehaviour
	{
		[Header("Despawn Animation Settings")]
		[SerializeField] private float _deathLayDuration = 2f;
		[SerializeField] private float _sinkDepth = 5f;
		[SerializeField] private float _sinkDuration = 1.5f; 
		[SerializeField] private AnimationCurve _sinkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    		
		private Vector3 _deathPosition;
		private Vector3 _sinkTargetPosition;
		private bool _isAnimating = false;
    		
		public bool IsAnimating => _isAnimating;
    		
		public async UniTaskVoid PlayDespawnAnimation(CancellationToken token)
		{
			if (_isAnimating)
				return;
    			
			_isAnimating = true;
    			
			_deathPosition = transform.position;
    			
			try
			{
				await UniTask.Delay(TimeSpan.FromSeconds(_deathLayDuration), cancellationToken: token);
    				
				_sinkTargetPosition = _deathPosition + Vector3.down * _sinkDepth;
    				
				float elapsed = 0f;
    				
				while (elapsed < _sinkDuration && !token.IsCancellationRequested)
				{
					elapsed += Time.deltaTime;
					float t = elapsed / _sinkDuration;
					float curveValue = _sinkCurve.Evaluate(t);
    					
					transform.position = Vector3.Lerp(_deathPosition, _sinkTargetPosition, curveValue);
    					
					await UniTask.Yield(token);
				}
    				
				if (!token.IsCancellationRequested)
				{
					Destroy(gameObject);
				}
			}
			catch (System.OperationCanceledException)
			{
				Destroy(gameObject);
			}
			finally
			{
				_isAnimating = false;
			}
		}
	}
}