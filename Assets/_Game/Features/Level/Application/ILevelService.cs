using System.Collections.Generic;

namespace Game.Features.Level.Application
{
    public interface ILevelService
    {
        string CurrentLevelId { get; }
        bool StartLevel(string levelId);
        void CompleteCurrentLevel();
        bool IsLevelUnlocked(string levelId);
        bool UnlockLevel(string levelId);
        IReadOnlyCollection<string> UnlockedLevels { get; }
        void Restore(string currentLevelId, IEnumerable<string> unlockedLevelIds);
    }
}
