using System;

namespace Game.Features.Shop.Domain
{
    [Serializable]
    public sealed class ShopOffer
    {
        public string OfferId;
        public string ItemId;
        public int ItemAmount = 1;
        public int Price = 10;
    }

    public readonly struct ItemPurchasedEvent : Game.Core.Events.IGameEvent
    {
        public ItemPurchasedEvent(string offerId, string itemId, int amount, int price)
        {
            OfferId = offerId;
            ItemId = itemId;
            Amount = amount;
            Price = price;
        }

        public string OfferId { get; }
        public string ItemId { get; }
        public int Amount { get; }
        public int Price { get; }
    }
}
