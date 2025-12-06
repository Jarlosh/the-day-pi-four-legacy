using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Client
{
	public class TutorialSequenceManager : MonoBehaviour
	{
		[Header("Sequence Steps")]
		[SerializeField] private List<TutorialStep> _steps = new List<TutorialStep>();
        
		[Header("Settings")]
		[SerializeField] private bool _autoStart = true;
		[SerializeField] private bool _skipOnRestart = false;
        
		private int _currentStepIndex = -1;
		private TutorialStep _currentStep;
		private CancellationTokenSource _sequenceCts;
		private bool _isSequenceActive;
        
		public bool IsSequenceActive => _isSequenceActive;
		public int CurrentStepIndex => _currentStepIndex;
        
		private void Start()
		{
			if (_autoStart)
			{
				StartSequence().Forget();
			}
		}
        
		private void OnDestroy()
		{
			_sequenceCts?.Cancel();
			_sequenceCts?.Dispose();
		}
        
		public async UniTaskVoid StartSequence()
		{
			if (_isSequenceActive || _steps.Count == 0)
				return;
            
			_isSequenceActive = true;
			_sequenceCts = new CancellationTokenSource();
			var token = _sequenceCts.Token;
            
			EventBus.Instance.Publish(new TutorialSequenceStartedEvent());
            
			try
			{
				for (int i = 0; i < _steps.Count; i++)
				{
					if (token.IsCancellationRequested)
						break;
                    
					_currentStepIndex = i;
					_currentStep = _steps[i];
                    
					if (_currentStep == null)
						continue;
                    
					_currentStep.Initialize();
                    
					await _currentStep.WaitForCompletion(token);
                    
					if (token.IsCancellationRequested)
						break;
                    
					_currentStep.Complete();
					EventBus.Instance.Publish(new TutorialStepCompletedEvent(i, _currentStep));
				}
                
				EventBus.Instance.Publish(new TutorialSequenceCompletedEvent());
			}
			catch (OperationCanceledException)
			{
				// Отменено
			}
			finally
			{
				_isSequenceActive = false;
			}
		}
        
		public void SkipToStep(int stepIndex)
		{
			if (stepIndex < 0 || stepIndex >= _steps.Count)
				return;
            
			if (_currentStep != null)
			{
				_currentStep.Complete();
			}
            
			_currentStepIndex = stepIndex;
		}
	}
}