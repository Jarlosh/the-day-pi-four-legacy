using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Game.Client.UI
{
	public class LocaleSelectorService: MonoBehaviour, ILocaleSelectorService
	{
		private const string LocalizationKey = "LocalKey";
		private bool _active = false;

		private void Start()
		{
			int localId = PlayerPrefs.GetInt(LocalizationKey, 0);
			ChangeLocale(localId);
		}

		public void ChangeLocale(int localeId)
		{
			if (_active)
			{
				return;
			}

			SetLocale(localeId).Forget();
		}

		private async UniTask SetLocale(int localeId)
		{
			if (_active)
			{
				Debug.LogWarning("[LocaleSelectorService] The localization is already changing.");
				return;
			}

			_active = true;
			Debug.Log($"[LocaleSelectorService] SetLocale: Starting Localization change to {localeId}");

			try
			{
				Debug.Log("[LocaleSelectorService] SetLocale: Wait for Init LocalizationSettings");
				await LocalizationSettings.InitializationOperation.Task;
				Debug.Log("[LocaleSelectorService] SetLocale: LocalizationSettings Initialized");

				if (localeId < 0 || localeId >= LocalizationSettings.AvailableLocales.Locales.Count)
				{
					Debug.LogError($"[LocaleSelectorService] SetLocale: Invalid localeId: {localeId}. Available {LocalizationSettings.AvailableLocales.Locales.Count} locales.");
					return;
				}

				LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeId];
				Debug.Log($"[LocaleSelectorService] SetLocale: Set selected Localization to {LocalizationSettings.SelectedLocale.LocaleName} id:{localeId}");

				PlayerPrefs.SetInt(LocalizationKey, localeId);
				PlayerPrefs.Save();

				Debug.Log("[LocaleSelectorService] SetLocale: Localization successfully changed");
			}
			catch (System.Exception e)
			{
				Debug.LogError($"[LocaleSelectorService] SetLocale: Error changing Localization: {e}");
			}
			finally
			{
				_active = false;
				Debug.Log("[LocaleSelectorService] SetLocale: Complete");
			}
		}
	}
}