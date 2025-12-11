using System;
using System.Collections.Generic;
using Game.Core;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Game.Client.App.Telemetry
{
	public class UnityAnalyticsService: IDisposable
	{
		private const string AnalyticsDevEnvironmentName = "dev";
		private const string AnalyticsProductionEnvironmentName = "production";

		private const string RunEndEventKey = "runEnd";
		private const string EnemyDeadEventKey = "enemyDead";
		private const string WaveStartedEventKey = "waveStarted";
		private const string WaveEndedEventKey = "waveEnded";
		private const string UpgradePickupEventKey = "upgradePickup";
		private const string TutorialCompleteEventKey = "tutorialComplete";
		private const string TutorialStepCompleteEventKey = "tutorialStepComplete";

		private const string RunResultParamKey = "runResult";
		private const string FinalScoreParamKey = "finalScore";
		private const string SuckedParamKey = "suckedObjectCount";

		private const string EnemyTypeParamKey = "enemyType";
		private const string UpgradeTypeParamKey = "upgradeType";
		private const string WaveNumberParamKey = "waveNumber";
		private const string StepIndexParamKey = "stepIndex";

		private bool _userConsent = false;

		public UnityAnalyticsService()
		{
			InitServices();

			EventBus.Instance.Subscribe<TutorialSequenceCompletedEvent>(OnTutorialCompleted);
			EventBus.Instance.Subscribe<TutorialStepCompletedEvent>(OnTutorialStepCompleted);

			EventBus.Instance.Subscribe<PickUpUpgrade>(OnPickUpUpgraded);
			EventBus.Instance.Subscribe<WaveStartedEvent>(OnWaveStarted);
			EventBus.Instance.Subscribe<WaveCompletedEvent>(OnWaveCompleted);
			EventBus.Instance.Subscribe<GameCancelEvent>(OnGameCanceled);
			EventBus.Instance.Subscribe<EnemyDeathEvent>(OnEnemyDead);
		}
		
		public void Dispose()
		{
			EventBus.Instance.Unsubscribe<TutorialSequenceCompletedEvent>(OnTutorialCompleted);
			EventBus.Instance.Unsubscribe<TutorialStepCompletedEvent>(OnTutorialStepCompleted);

			EventBus.Instance.Unsubscribe<PickUpUpgrade>(OnPickUpUpgraded);
			EventBus.Instance.Unsubscribe<WaveCompletedEvent>(OnWaveCompleted);
			EventBus.Instance.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
			EventBus.Instance.Unsubscribe<EnemyDeathEvent>(OnEnemyDead);
			EventBus.Instance.Unsubscribe<GameCancelEvent>(OnGameCanceled);

			_userConsent = false;
		}

		private async void InitServices()
		{
			try
			{
				var options = new InitializationOptions();

				#if UNITY_EDITOR || DEVELOPMENT_BUILD
				options.SetEnvironmentName(AnalyticsDevEnvironmentName);
				#else
				options.SetEnvironmentName(AnalyticsProductionEnvironmentName);
				#endif

				await UnityServices.InitializeAsync(options);
				UserGiveConsent();
			}
			catch (Exception e)
			{
				Debug.Log(e.ToString());
			}
		}

		private void UserGiveConsent()
		{
			AnalyticsService.Instance.StartDataCollection();
			_userConsent = true;
			Debug.Log($"Consent has been provided. The SDK is now collecting data!");
		}

		private void OnTutorialCompleted(TutorialSequenceCompletedEvent evt)
		{
			if (!_userConsent)
			{
				return;
			}

			var customEvent = new CustomEvent(TutorialCompleteEventKey);
			AnalyticsService.Instance.RecordEvent(customEvent);
		}

		private void OnTutorialStepCompleted(TutorialStepCompletedEvent evt)
		{
			if (!_userConsent)
			{
				return;
			}

			var parameters = new Dictionary<string, object>()
			{
				{StepIndexParamKey, evt.StepIndex},
			};
			var customEvent = new CustomEvent(TutorialStepCompleteEventKey);
			foreach (var param in parameters)
			{
				customEvent.Add(param.Key, param.Value);
			}

			AnalyticsService.Instance.RecordEvent(customEvent);
		}

		private void OnGameCanceled(GameCancelEvent evt)
		{
			if (!_userConsent)
			{
				return;
			}

			var styleSystem = ServiceLocator.Get<StyleSystem>();

			var parameters = new Dictionary<string, object>()
			{
				{RunResultParamKey, evt.Results.ToString()},
				{FinalScoreParamKey, Mathf.FloorToInt(evt.FinalScore)},
				{SuckedParamKey, styleSystem != null ? styleSystem.VacuumedObjectsCount : -1}
			};

			var customEvent = new CustomEvent(RunEndEventKey);
			foreach (var param in parameters)
			{
				customEvent.Add(param.Key, param.Value);
			}

			AnalyticsService.Instance.RecordEvent(customEvent);
			AnalyticsService.Instance.Flush();
		}

		private void OnEnemyDead(EnemyDeathEvent evt)
		{
			if (!_userConsent)
			{
				return;
			}

			var parameters = new Dictionary<string, object>()
			{
				{EnemyTypeParamKey, evt.Enemy.GetType().Name},
			};

			var customEvent = new CustomEvent(EnemyDeadEventKey);
			foreach (var param in parameters)
			{
				customEvent.Add(param.Key, param.Value);
			}

			AnalyticsService.Instance.RecordEvent(customEvent);
		}

		private void OnWaveCompleted(WaveCompletedEvent evt)
		{
			if (!_userConsent)
			{
				return;
			}

			var parameters = new Dictionary<string, object>()
			{
				{WaveNumberParamKey, evt.WaveNumber}
			};

			var customEvent = new CustomEvent(WaveEndedEventKey);
			foreach (var param in parameters)
			{
				customEvent.Add(param.Key, param.Value);
			}

			AnalyticsService.Instance.RecordEvent(customEvent);
		}

		private void OnWaveStarted(WaveStartedEvent evt)
		{
			if (!_userConsent)
			{
				return;
			}

			var parameters = new Dictionary<string, object>()
			{
				{WaveNumberParamKey, evt.WaveNumber}
			};


			var customEvent = new CustomEvent(WaveStartedEventKey);
			foreach (var param in parameters)
			{
				customEvent.Add(param.Key, param.Value);
			}

			AnalyticsService.Instance.RecordEvent(customEvent);
		}

		private void OnPickUpUpgraded(PickUpUpgrade evt)
		{
			if (!_userConsent)
			{
				return;
			}

			var parameters = new Dictionary<string, object>()
			{
				{UpgradeTypeParamKey, evt.Type.ToString()}
			};

			var customEvent = new CustomEvent(UpgradePickupEventKey);
			foreach (var param in parameters)
			{
				customEvent.Add(param.Key, param.Value);
			}

			AnalyticsService.Instance.RecordEvent(customEvent);
		}
	}
}