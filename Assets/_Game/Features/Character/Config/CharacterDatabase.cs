using System.Collections.Generic;
using UnityEngine;

namespace Game.Features.Character.Config
{
    [CreateAssetMenu(menuName = "Game/Character/Character Database", fileName = "CharacterDatabase")]
    public sealed class CharacterDatabase : ScriptableObject
    {
        [field: SerializeField] public List<CharacterDefinition> Characters { get; private set; } = new();
    }
}
