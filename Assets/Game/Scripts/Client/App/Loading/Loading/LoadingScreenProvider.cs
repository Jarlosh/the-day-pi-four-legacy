using System.Threading.Tasks;
using Game.Client.UI;

namespace Game.Client.App
{
    public class LoadingScreenProvider: LocalSceneLoader
    {
        public Task<LoadingScreen> Load()
        {
            return LoadingInternal<LoadingScreen>(ApplicationSettings.LoadingScene);
        }

        public void Unload()
        {
            UnloadInternal();
        }
    }
}