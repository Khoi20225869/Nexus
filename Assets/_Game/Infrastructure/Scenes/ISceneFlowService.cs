namespace Game.Infrastructure.Scenes
{
    public interface ISceneFlowService
    {
        string MetaSceneName { get; }
        string GameplaySceneName { get; }
        void LoadMetaScene();
        void LoadGameplayScene();
        void LoadScene(string sceneName);
    }
}
