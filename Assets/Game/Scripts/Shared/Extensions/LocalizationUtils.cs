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
                return cachedString;
            }

            if (_stringLoadTasks.TryGetValue(keyId, out var existingTask))
            {
                var result = await existingTask;
                
                if (!string.IsNullOrEmpty(result))
                {
                    _stringCache[keyId] = result;
                }
                _stringLoadTasks.Remove(keyId);
                
                return result;
            }

            var loadTask = GetStringFromLocalizedString(localizedString);
            _stringLoadTasks[keyId] = loadTask;

            try
            {
                var result = await loadTask;
                
                if (!string.IsNullOrEmpty(result))
                {
                    _stringCache[keyId] = result;
                }
                
                return result;
            }
            finally
            {
                // Удаляем задачу из словаря после завершения
                _stringLoadTasks.Remove(keyId);
            }
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