namespace Game.Features.Economy.Application
{
    public interface ICurrencyService
    {
        int Gold { get; }
        void AddGold(int amount);
        bool SpendGold(int amount);
        void SetGold(int value);
    }
}
