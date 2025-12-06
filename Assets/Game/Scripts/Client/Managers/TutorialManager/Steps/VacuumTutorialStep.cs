using System.Collections.Generic;
using UnityEngine;

namespace Game.Client
{
	public class VacuumTutorialStep : TutorialStep
	{
		[Header("Vacuum Tutorial")]
		[SerializeField] private List<VacuumedObject> _objectsToVacuum = new();
		[SerializeField] private int _requiredObjectsCount = 7;
		[SerializeField] private GameObject _blockingDebris;
        
		private int _vacuumedCount;
        
		protected override void OnInitialize()
		{
			base.OnInitialize();
            
			_vacuumedCount = 0;
            
			EventBus.Instance.Subscribe<VacuumSuccessEvent>(OnVacuumSuccess);
			
			if (_blockingDebris != null)
			{
				_blockingDebris.SetActive(true);
			}
		}
        
		protected override void OnComplete()
		{
			base.OnComplete();
            
			EventBus.Instance.Unsubscribe<VacuumSuccessEvent>(OnVacuumSuccess);
            
			if (_blockingDebris != null)
			{
				_blockingDebris.SetActive(false);
			}
		}
        
		private void OnVacuumSuccess(VacuumSuccessEvent _)
		{
			_vacuumedCount++;
            
			if (_vacuumedCount >= _requiredObjectsCount)
			{
				Complete();
			}
		}
	}
}