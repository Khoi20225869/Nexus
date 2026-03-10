using System.Collections.Generic;

namespace Game.Features.Character.Application
{
    public interface ICharacterService
    {
        IReadOnlyCollection<string> UnlockedCharacterIds { get; }
        string ActiveCharacterId { get; }
        bool Unlock(string characterId);
        bool Switch(string characterId);
        bool IsUnlocked(string characterId);
        void Restore(string activeCharacterId, IEnumerable<string> unlockedCharacterIds);
    }
}
