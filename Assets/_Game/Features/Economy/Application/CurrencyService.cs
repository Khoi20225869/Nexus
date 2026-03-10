using Game.Core.Events;
using Game.Features.Economy.Domain;

namespace Game.Features.Economy.Application
{
    public sealed class CurrencyService : ICurrencyService
    {
        private readonly IGameEventBus _eventBus;

        public CurrencyService(IGameEventBus eventBus, int startingGold)
        {
            _eventBus = eventBus;
            Gold = startingGold < 0 ? 0 : startingGold;
        }

        public int Gold { get; private set; }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Gold += amount;
            _eventBus.Publish(new GoldChangedEvent(Gold, amount));
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0 || Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            _eventBus.Publish(new GoldChangedEvent(Gold, -amount));
            return true;
        }

        public void SetGold(int value)
        {
            var clamped = value < 0 ? 0 : value;
            var delta = clamped - Gold;
            Gold = clamped;
            _eventBus.Publish(new GoldChangedEvent(Gold, delta));
        }
    }
}
