using Game.Infrastructure.Scenes;
using UnityEngine;

namespace Game.Bootstrap
{
    public sealed class GameStartupController : MonoBehaviour
    {
        [SerializeField] private bool forceLoadMetaOnStart;

        private void Start()
        {
            if (!forceLoadMetaOnStart)
            {
                return;
            }

            var sceneFlow = GameInstaller.Services?.Resolve<ISceneFlowService>();
            sceneFlow?.LoadMetaScene();
        }
    }
}
