using UnityEngine;

namespace Game.Features.Level.Config
{
    [CreateAssetMenu(menuName = "Game/Level/Level Definition", fileName = "LevelDefinition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string SceneName { get; private set; }
        [field: SerializeField] public int RecommendedPower { get; private set; }
    }
}
