using UnityEngine;
using UnityEngine.Localization;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization.Tables;

namespace Game.Shared
{
    public class LocalizationUtils
    {
        private static readonly Dictionary<TableEntryReference, UniTask<string>> _stringLoadTasks = new();
        private static readonly Dictionary<TableEntryReference, string> _stringCache = new();
        
        public static async UniTask<string> GetLocalizedString(LocalizedString localizedString)
        {
            if (localizedString == null)
            {
                return string.Empty;
            }

            var keyId = localizedString.TableEntryReference;
            if (_stringCache.TryGetValue(keyId, out string cachedString))
            {
                UnityEngine.Debug.Log($"LocalizationUtility: Using cached row from cache: {keyId}");
                return cachedString;
            }

            if (_stringLoadTasks.TryGetValue(keyId, out var existingTask))
            {
                UnityEngine.Debug.Log($"LocalizationUtility: Using cached task to load row: {keyId}");
                return await existingTask;
            }

            UnityEngine.Debug.Log($"LocalizationUtility: Loading rowId: {keyId}");

            var loadTask = GetStringFromLocalizedString(localizedString);

            _stringLoadTasks[keyId] = loadTask;

            var result = await loadTask;

            _stringCache[keyId] = result;

            return result;
        }

        private static async UniTask<string> GetStringFromLocalizedString(LocalizedString localizedString)
        {
            try
            {
                return await localizedString.GetLocalizedStringAsync();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"LocalizationUtility: Error loading row: {e}");
                return string.Empty;
            }
            finally
            {
                // We do nothing in finally, because UniTask will free up resources itself.
            }
        }

        public static void ClearCache()
        {
            UnityEngine.Debug.Log("LocalizationUtility: Clearing cache.");
            _stringLoadTasks.Clear();
            _stringCache.Clear();
        }
    }
}