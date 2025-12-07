using Game.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Game.Client.UI
{
	public class ShootModeView : UIViewBase
	{
		[Header("References")]
		[SerializeField] private VacuumGun _vacuumGun;
		[SerializeField] private TextMeshProUGUI _modeText;
        
		[Header("Settings")]
		[SerializeField] private string _modeFormat = "Mode: {0}";
		[SerializeField] private LocalizedString _singleModeName;
		[SerializeField] private LocalizedString _shotgunModeName;

		private VacuumGun.ShootMode _currentMode = VacuumGun.ShootMode.Single;
		
		protected override void Init()
		{
			base.Init();
            
			if (_vacuumGun == null)
			{
				_vacuumGun = FindFirstObjectByType<VacuumGun>();
			}
            
			if (_vacuumGun != null)
			{
				UpdateMode(_vacuumGun.CurrentShootMode);
			}
		}
        
		protected override void Subscribe()
		{
			base.Subscribe();
            
			EventBus.Instance.Subscribe<ShootModeChangedEvent>(OnShootModeChanged);
			LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
		}
        
		protected override void Unsubscribe()
		{
			base.Unsubscribe();
            
			EventBus.Instance.Unsubscribe<ShootModeChangedEvent>(OnShootModeChanged);
			LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
		}

		private void OnSelectedLocaleChanged(Locale locale)
		{
			UpdateMode(_currentMode);
		}

		private void OnShootModeChanged(ShootModeChangedEvent evt)
		{
			_currentMode = (VacuumGun.ShootMode)evt.ModeIndex;
			UpdateMode(_currentMode);
		}
        
		private void UpdateMode(VacuumGun.ShootMode mode)
		{
			if (_modeText == null)
				return;
            
			string modeName = mode == VacuumGun.ShootMode.Single ? LocalizationUtils.GetLocalizedString(_singleModeName).ToString() : LocalizationUtils.GetLocalizedString(_shotgunModeName).ToString();
			_modeText.text = string.Format(_modeFormat, modeName);
		}
	}
}