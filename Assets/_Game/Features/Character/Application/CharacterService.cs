using System.Collections.Generic;
using Game.Core.Events;
using Game.Features.Character.Domain;

namespace Game.Features.Character.Application
{
    public sealed class CharacterService : ICharacterService
    {
        private readonly HashSet<string> _unlocked = new();
        private readonly IGameEventBus _eventBus;

        public CharacterService(IGameEventBus eventBus, string starterCharacterId)
        {
            _eventBus = eventBus;
            if (!string.IsNullOrWhiteSpace(starterCharacterId))
            {
                _unlocked.Add(starterCharacterId);
                ActiveCharacterId = starterCharacterId;
            }
        }

        public IReadOnlyCollection<string> UnlockedCharacterIds => _unlocked;
        public string ActiveCharacterId { get; private set; }

        public bool Unlock(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return false;
            }

            return _unlocked.Add(characterId);
        }

        public bool Switch(string characterId)
        {
            if (!_unlocked.Contains(characterId))
            {
                return false;
            }

            ActiveCharacterId = characterId;
            _eventBus.Publish(new CharacterSwitchedEvent(characterId));
            return true;
        }

        public bool IsUnlocked(string characterId)
        {
            return _unlocked.Contains(characterId);
        }

        public void Restore(string activeCharacterId, IEnumerable<string> unlockedCharacterIds)
        {
            _unlocked.Clear();
            if (unlockedCharacterIds != null)
            {
                foreach (var id in unlockedCharacterIds)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        _unlocked.Add(id);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(activeCharacterId))
            {
                _unlocked.Add(activeCharacterId);
                ActiveCharacterId = activeCharacterId;
            }
            else
            {
                ActiveCharacterId = string.Empty;
            }
        }
    }
}
