using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Client
{
	[Serializable]
	public abstract class TutorialStep : MonoBehaviour
	{
		[Header("Step Settings")]
		[SerializeField] protected string _stepName = "Step";
		[SerializeField] protected bool _autoComplete = false;
        
		protected bool _isCompleted;
		protected bool _isInitialized;
        
		public string StepName => _stepName;
		public bool IsCompleted => _isCompleted;
        
		public virtual void Initialize()
		{
			if (_isInitialized)
				return;
            
			_isInitialized = true;
			_isCompleted = false;
			OnInitialize();
		}
        
		public virtual void Complete()
		{
			if (_isCompleted)
				return;
            
			_isCompleted = true;
			OnComplete();
		}
        
		public async UniTask WaitForCompletion(CancellationToken token)
		{
			while (!_isCompleted && !token.IsCancellationRequested)
			{
				if (_autoComplete)
				{
					CheckAutoComplete();
				}
                
				await UniTask.Yield(token);
			}
		}
        
		protected virtual void OnInitialize()
		{
		}
        
		protected virtual void OnComplete()
		{
		}
        
		protected virtual void CheckAutoComplete()
		{
		}
	}
}