namespace Game.Features.Inventory.Domain
{
    public readonly struct InventoryChangedEvent : Game.Core.Events.IGameEvent
    {
        public InventoryChangedEvent(string itemId, int delta)
        {
            ItemId = itemId;
            Delta = delta;
        }

        public string ItemId { get; }
        public int Delta { get; }
    }
}
