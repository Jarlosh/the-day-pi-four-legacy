using Game.Client.App.Telemetry;
using Game.Client.UI;
using Game.Core;
using Game.Shared.Singletons;
using UnityEngine;

#if UNITY_EDITOR
using NUnit.Framework;
#endif

namespace Game.Client.App
{
    public class ProjectContext : AutoSingletonBehaviour<ProjectContext>
    {
        private const string ProjectContextResourcePath = "ProjectContext";

        [SerializeField] private LocaleSelectorService _localeSelectorService;
        [SerializeField] private TimeService _timeService;
        [SerializeField] private MusicManager _musicManager;

        private UnityAnalyticsService _analyticsService;
        
        [RuntimeInitializeOnLoadMethod]
        private static void OnLoadMethod()
        {
            var prefab = TryGetPrefab();
            InstantiateAutoSingleton(prefab);
        }
        
        protected override void OnInit()
        {
            ServiceLocator.Register<ILocaleSelectorService>(_localeSelectorService);
            ServiceLocator.Register<ITimeService>(_timeService);
            ServiceLocator.Register(_musicManager);
            
            _analyticsService = ServiceLocator.Register(new UnityAnalyticsService());

            Application.targetFrameRate = 120;
        }

        protected override void OnRelease()
        {
            _analyticsService.Dispose();
            
            ServiceLocator.Clear();
        }
        
        private static GameObject TryGetPrefab()
        {
            var prefabs = Resources.LoadAll(ProjectContextResourcePath, typeof(GameObject));

            if (prefabs.Length > 0)
            {
#if UNITY_EDITOR
                Assert.That(prefabs.Length == 1,
                    "Found multiple project context prefabs at resource path '{0}'", ProjectContextResourcePath);
#endif
                return (GameObject)prefabs[0];
            }
            
            return null;
        }

    }
}
