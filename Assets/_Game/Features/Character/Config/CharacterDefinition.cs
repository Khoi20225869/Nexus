using UnityEngine;

namespace Game.Features.Character.Config
{
    [CreateAssetMenu(menuName = "Game/Character/Character Definition", fileName = "CharacterDefinition")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }
    }
}
