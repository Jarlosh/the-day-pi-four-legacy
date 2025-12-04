using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Game.Client.UI
{

    public class LocaleSelector: MonoBehaviour
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

            SetLocale(localeId);
        }

        private async UniTask SetLocale(int localeId)
        {
            if (_active)
            {
                Debug.LogWarning("The localization is already changing.");
                return;
            }

            _active = true;
            Debug.Log($"SetLocale: Starting Localization change to {localeId}");

            try
            {
                Debug.Log("SetLocale: Wait for Init LocalizationSettings");
                await LocalizationSettings.InitializationOperation.Task;
                Debug.Log("SetLocale: LocalizationSettings Initialized");

                if (localeId < 0 || localeId >= LocalizationSettings.AvailableLocales.Locales.Count)
                {
                    Debug.LogError($"SetLocale: Invalid localeId: {localeId}. Available {LocalizationSettings.AvailableLocales.Locales.Count} locales.");
                    return;
                }

                Debug.Log($"SetLocale: Set selected Localization to {localeId}");
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeId];

                PlayerPrefs.SetInt(LocalizationKey, localeId);
                PlayerPrefs.Save();

                Debug.Log("SetLocale: Localization successfully changed");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SetLocale: Error changing Localization: {e}");
            }
            finally
            {
                _active = false;
                Debug.Log("SetLocale: Complete");
            }
        }
    }
}