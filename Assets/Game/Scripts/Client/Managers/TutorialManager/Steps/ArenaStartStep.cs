using UnityEngine;

namespace Game.Client
{
	public class ArenaStartStep : TutorialStep
	{
		[Header("Arena Settings")]
		[SerializeField] private WaveManager _waveManager;
        
		protected override void OnInitialize()
		{
			base.OnInitialize();
			
			if (_waveManager != null)
			{
				_autoComplete = true;
			}
		}
        
		protected override void CheckAutoComplete()
		{
			if (_waveManager != null)
			{
				Complete();
			}
		}

		protected override void OnComplete()
		{
			_waveManager.StartGame().Forget();
		}
	}
}