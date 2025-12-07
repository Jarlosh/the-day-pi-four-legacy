using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client.UI
{
	public class VacuumAmmoView : UIViewBase
	{
		[Header("References")]
		[SerializeField] private VacuumGun _vacuumGun;
		[SerializeField] private TextMeshProUGUI _ammoText;
		[SerializeField] private Slider _ammoProgressBar;
        
		[Header("Settings")]
		[SerializeField] private string _ammoFormat = "{0}/{1}";
        
		protected override void Init()
		{
			base.Init();
            
			if (_vacuumGun == null)
			{
				_vacuumGun = FindFirstObjectByType<VacuumGun>();
			}
            
			if (_vacuumGun != null)
			{
				UpdateAmmo(_vacuumGun.VacuumedObjectsCount, _vacuumGun.MaxObjects);
			}
		}
        
		protected override void Subscribe()
		{
			base.Subscribe();
            
			EventBus.Instance.Subscribe<VacuumedObjectsChangedEvent>(OnVacuumedObjectsChanged);
		}
        
		protected override void Unsubscribe()
		{
			base.Unsubscribe();
            
			EventBus.Instance.Unsubscribe<VacuumedObjectsChangedEvent>(OnVacuumedObjectsChanged);
		}
        
		private void OnVacuumedObjectsChanged(VacuumedObjectsChangedEvent evt)
		{
			UpdateAmmo(evt.CurrentCount, evt.MaxCount);
		}
        
		private void UpdateAmmo(int currentCount, int maxCount)
		{
			if (_ammoProgressBar != null)
			{
				_ammoProgressBar.maxValue = maxCount;
				_ammoProgressBar.value = currentCount;
			}
            
			if (_ammoText != null)
			{
				_ammoText.text = string.Format(_ammoFormat, currentCount, maxCount);
			}
		}
	}
}