using UnityEngine.SceneManagement;

namespace Game.Infrastructure.Scenes
{
    public sealed class UnitySceneFlowService : ISceneFlowService
    {
        public UnitySceneFlowService(string metaSceneName, string gameplaySceneName)
        {
            MetaSceneName = metaSceneName;
            GameplaySceneName = gameplaySceneName;
        }

        public string MetaSceneName { get; }
        public string GameplaySceneName { get; }

        public void LoadMetaScene()
        {
            LoadScene(MetaSceneName);
        }

        public void LoadGameplayScene()
        {
            LoadScene(GameplaySceneName);
        }

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
