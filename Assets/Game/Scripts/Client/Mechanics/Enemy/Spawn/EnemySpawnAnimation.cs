using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Client
{
	public class EnemySpawnAnimation: MonoBehaviour
	{
		[Header("Spawn Animation Settings")] [SerializeField]
		private float _spawnDepth = 2f;

		[SerializeField] private float _spawnDuration = 1f;
		[SerializeField] private AnimationCurve _spawnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		private Vector3 _targetPosition;
		private Vector3 _startPosition;
		private bool _isAnimating = false;

		public bool IsAnimating => _isAnimating;

		public async UniTaskVoid PlaySpawnAnimation(CancellationToken token)
		{
			if (_isAnimating)
				return;

			_isAnimating = true;

			_targetPosition = transform.localPosition;

			_startPosition = _targetPosition + Vector3.down * _spawnDepth;
			transform.localPosition = _startPosition;

			float elapsed = 0f;

			try
			{
				while (elapsed < _spawnDuration && !token.IsCancellationRequested)
				{
					elapsed += Time.deltaTime;
					float t = elapsed / _spawnDuration;
					float curveValue = _spawnCurve.Evaluate(t);

					transform.localPosition = Vector3.Lerp(_startPosition, _targetPosition, curveValue);

					await UniTask.Yield(token);
				}

				if (!token.IsCancellationRequested)
				{
					transform.localPosition = _targetPosition;
				}
			}
			catch (System.OperationCanceledException)
			{
				// Анимация отменена
			}
			finally
			{
				_isAnimating = false;
			}
		}
	}
}