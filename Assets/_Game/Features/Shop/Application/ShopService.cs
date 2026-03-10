using System.Collections.Generic;
using Game.Core.Events;
using Game.Features.Economy.Application;
using Game.Features.Inventory.Application;
using Game.Features.Shop.Config;
using Game.Features.Shop.Domain;

namespace Game.Features.Shop.Application
{
    public sealed class ShopService : IShopService
    {
        private readonly Dictionary<string, ShopOffer> _offersById = new();
        private readonly ICurrencyService _currencyService;
        private readonly IInventoryService _inventoryService;
        private readonly IGameEventBus _eventBus;

        public ShopService(
            ShopCatalog catalog,
            ICurrencyService currencyService,
            IInventoryService inventoryService,
            IGameEventBus eventBus)
        {
            _currencyService = currencyService;
            _inventoryService = inventoryService;
            _eventBus = eventBus;

            if (catalog == null)
            {
                return;
            }

            foreach (var offer in catalog.Offers)
            {
                if (!string.IsNullOrWhiteSpace(offer?.OfferId))
                {
                    _offersById[offer.OfferId] = offer;
                }
            }
        }

        public IReadOnlyList<ShopOffer> GetOffers()
        {
            var list = new List<ShopOffer>(_offersById.Values.Count);
            foreach (var offer in _offersById.Values)
            {
                list.Add(offer);
            }

            return list;
        }

        public PurchaseResult TryPurchase(string offerId)
        {
            if (!_offersById.TryGetValue(offerId, out var offer))
            {
                return PurchaseResult.OfferNotFound;
            }

            if (!_currencyService.SpendGold(offer.Price))
            {
                return PurchaseResult.NotEnoughGold;
            }

            if (!_inventoryService.AddItem(offer.ItemId, offer.ItemAmount))
            {
                _currencyService.AddGold(offer.Price);
                return PurchaseResult.InventoryRejected;
            }

            _eventBus.Publish(new ItemPurchasedEvent(offer.OfferId, offer.ItemId, offer.ItemAmount, offer.Price));
            return PurchaseResult.Success;
        }
    }
}
