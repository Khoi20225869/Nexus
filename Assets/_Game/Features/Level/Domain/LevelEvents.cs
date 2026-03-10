namespace Game.Features.Level.Domain
{
    public readonly struct LevelStartedEvent : Game.Core.Events.IGameEvent
    {
        public LevelStartedEvent(string levelId)
        {
            LevelId = levelId;
        }

        public string LevelId { get; }
    }

    public readonly struct LevelCompletedEvent : Game.Core.Events.IGameEvent
    {
        public LevelCompletedEvent(string levelId)
        {
            LevelId = levelId;
        }

        public string LevelId { get; }
    }
}
