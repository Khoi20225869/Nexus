using UnityEngine;

namespace Game.Bootstrap
{
    [CreateAssetMenu(menuName = "Game/Bootstrap/Game Bootstrap Config", fileName = "GameBootstrapConfig")]
    public sealed class GameBootstrapConfig : ScriptableObject
    {
        [field: SerializeField] public string MetaSceneName { get; private set; } = "Meta";
        [field: SerializeField] public string GameplaySceneName { get; private set; } = "Gameplay";
        [field: SerializeField] public bool AutoLoadMetaOnStart { get; private set; }
        [field: SerializeField] public string SaveFileName { get; private set; } = "player_progress.json";
    }
}
