using System.Collections.Generic;
using Game.Features.Shop.Domain;

namespace Game.Features.Shop.Application
{
    public enum PurchaseResult
    {
        Success = 0,
        OfferNotFound = 1,
        NotEnoughGold = 2,
        InventoryRejected = 3
    }

    public interface IShopService
    {
        IReadOnlyList<ShopOffer> GetOffers();
        PurchaseResult TryPurchase(string offerId);
    }
}
