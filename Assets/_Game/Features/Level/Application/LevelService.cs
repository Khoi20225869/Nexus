using System.Collections.Generic;
using Game.Core.Events;
using Game.Features.Level.Domain;

namespace Game.Features.Level.Application
{
    public sealed class LevelService : ILevelService
    {
        private readonly HashSet<string> _unlockedLevels = new();
        private readonly IGameEventBus _eventBus;

        public LevelService(IGameEventBus eventBus, string starterLevelId)
        {
            _eventBus = eventBus;
            if (!string.IsNullOrWhiteSpace(starterLevelId))
            {
                _unlockedLevels.Add(starterLevelId);
                CurrentLevelId = starterLevelId;
            }
        }

        public string CurrentLevelId { get; private set; }
        public IReadOnlyCollection<string> UnlockedLevels => _unlockedLevels;

        public bool StartLevel(string levelId)
        {
            if (string.IsNullOrWhiteSpace(levelId) || !_unlockedLevels.Contains(levelId))
            {
                return false;
            }

            CurrentLevelId = levelId;
            _eventBus.Publish(new LevelStartedEvent(levelId));
            return true;
        }

        public void CompleteCurrentLevel()
        {
            if (string.IsNullOrWhiteSpace(CurrentLevelId))
            {
                return;
            }

            _eventBus.Publish(new LevelCompletedEvent(CurrentLevelId));
        }

        public bool IsLevelUnlocked(string levelId)
        {
            return _unlockedLevels.Contains(levelId);
        }

        public bool UnlockLevel(string levelId)
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                return false;
            }

            return _unlockedLevels.Add(levelId);
        }

        public void Restore(string currentLevelId, IEnumerable<string> unlockedLevelIds)
        {
            _unlockedLevels.Clear();
            if (unlockedLevelIds != null)
            {
                foreach (var id in unlockedLevelIds)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        _unlockedLevels.Add(id);
                    }
                }
            }

            CurrentLevelId = string.IsNullOrWhiteSpace(currentLevelId) ? string.Empty : currentLevelId;
            if (!string.IsNullOrWhiteSpace(CurrentLevelId))
            {
                _unlockedLevels.Add(CurrentLevelId);
            }
        }
    }
}
