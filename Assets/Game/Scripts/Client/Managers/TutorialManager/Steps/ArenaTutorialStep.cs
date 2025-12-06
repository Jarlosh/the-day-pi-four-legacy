using UnityEngine;

namespace Game.Client
{
	public class ArenaTutorialStep : TutorialStep
	{
		[Header("Arena Settings")]
		[SerializeField] private WaveManager _waveManager;
		[SerializeField] private GameObject _arenaGate;
        
		protected override void OnInitialize()
		{
			base.OnInitialize();
			
			if (_arenaGate != null)
			{
				_arenaGate.SetActive(false);
			}
			
			if (_waveManager != null)
			{
				_autoComplete = true;
			}
		}
        
		protected override void CheckAutoComplete()
		{
			if (_waveManager != null && _waveManager.IsGameActive)
			{
				Complete();
			}
		}
	}
}