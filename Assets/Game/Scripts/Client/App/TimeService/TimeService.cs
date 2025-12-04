using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Client.App
{
	public class TimeService: MonoBehaviour, ITimeService
	{
		public event Action<bool> EventPaused;

		[SerializeField, UsedImplicitly] private float _timeScaleChangeSpeed = -1;

		[SerializeField, UsedImplicitly] private float _pausedTimeScale = 0;

		[SerializeField, UsedImplicitly] private float _activeTimeScale = 1;

		private readonly List<object> _pauseSources = new();
		private readonly Dictionary<object, PauseType> _pauseSourcesTypes = new();

		public float DeltaTime => Time.deltaTime;
		public float TimeScalePaused => _pausedTimeScale;
		public float TimeScaleActive => _activeTimeScale;
		public float TimeScaleCurrent => Time.timeScale;
		public float TimeScaleTarget => IsPaused ? _pausedTimeScale : _activeTimeScale;
		public bool IsPaused { get; private set; }
		public PauseType PauseType { get; private set; }

		private void Awake() => UpdatePauseStateAsync(destroyCancellationToken).SuppressCancellationThrow().Forget();

		private void OnDestroy()
		{
			Time.timeScale = 1;
		}

		private void Update()
		{
			if (!Mathf.Approximately(TimeScaleCurrent, TimeScaleTarget))
			{
				if (_timeScaleChangeSpeed <= 0)
				{
					Time.timeScale = TimeScaleTarget;
				}
				else
				{
					Time.timeScale = Mathf.MoveTowards(Time.timeScale, TimeScaleTarget, _timeScaleChangeSpeed * Time.unscaledDeltaTime);
				}
			}
		}

		private async UniTask UpdatePauseStateAsync(CancellationToken token)
		{
			while (true)
			{
				await UniTask.Yield(PlayerLoopTiming.PreLateUpdate, token);

				var lastValue = IsPaused;
				IsPaused = _pauseSources.Count > 0;

				if (IsPaused)
				{
					PauseType = _pauseSourcesTypes.Values.Max();
				}

				if (lastValue != IsPaused)
				{
					EventPaused?.Invoke(IsPaused);
				}
			}
		}

		public void Pause(object source, PauseType pauseType = PauseType.Unknown)
		{
			if (_pauseSources.Contains(source))
			{
				Debug.LogError("[TimeService] PauseSource contains source should be False");
			}

			_pauseSources.Add(source);
			_pauseSourcesTypes.Add(source, pauseType);
		}

		public void Resume(object source)
		{
			if (_pauseSources.Contains(source))
			{
				Debug.LogError("[TimeService] PauseSource contains source should be True");
			}

			_pauseSources.Remove(source);
			_pauseSourcesTypes.Remove(source);
		}

		public void SetTimeScale(float timeScale)
		{
			_activeTimeScale = timeScale;
		}

		public void SubscribePause(Action<bool> pauseAction)
		{
			EventPaused += pauseAction;

			if (IsPaused)
			{
				pauseAction?.Invoke(true);
			}
		}

		public void UnsubscribePause(Action<bool> pauseAction)
		{
			EventPaused -= pauseAction;

			if (IsPaused)
			{
				pauseAction?.Invoke(false);
			}
		}
	}
}