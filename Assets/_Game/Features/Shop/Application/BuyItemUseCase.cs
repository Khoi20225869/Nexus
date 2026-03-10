namespace Game.Features.Shop.Application
{
    public sealed class BuyItemUseCase
    {
        private readonly IShopService _shopService;

        public BuyItemUseCase(IShopService shopService)
        {
            _shopService = shopService;
        }

        public PurchaseResult Execute(string offerId)
        {
            return _shopService.TryPurchase(offerId);
        }
    }
}
