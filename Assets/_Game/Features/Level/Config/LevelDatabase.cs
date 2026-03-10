using System.Collections.Generic;
using UnityEngine;

namespace Game.Features.Level.Config
{
    [CreateAssetMenu(menuName = "Game/Level/Level Database", fileName = "LevelDatabase")]
    public sealed class LevelDatabase : ScriptableObject
    {
        [field: SerializeField] public List<LevelDefinition> Levels { get; private set; } = new();

        public LevelDefinition GetById(string id)
        {
            for (var i = 0; i < Levels.Count; i++)
            {
                var level = Levels[i];
                if (level != null && level.Id == id)
                {
                    return level;
                }
            }

            return null;
        }
    }
}
