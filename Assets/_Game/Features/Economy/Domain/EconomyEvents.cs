using Game.Core.Events;

namespace Game.Features.Economy.Domain
{
    public readonly struct GoldChangedEvent : IGameEvent
    {
        public GoldChangedEvent(int currentGold, int delta)
        {
            CurrentGold = currentGold;
            Delta = delta;
        }

        public int CurrentGold { get; }
        public int Delta { get; }
    }
}
