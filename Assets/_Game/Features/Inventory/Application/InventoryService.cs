using System.Collections.Generic;
using Game.Core.Events;
using Game.Features.Inventory.Domain;

namespace Game.Features.Inventory.Application
{
    public sealed class InventoryService : IInventoryService
    {
        private readonly Dictionary<string, int> _items = new();
        private readonly IGameEventBus _eventBus;

        public InventoryService(IGameEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public bool AddItem(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return false;
            }

            _items.TryGetValue(itemId, out var current);
            _items[itemId] = current + amount;
            _eventBus.Publish(new InventoryChangedEvent(itemId, amount));
            return true;
        }

        public bool RemoveItem(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return false;
            }

            if (!_items.TryGetValue(itemId, out var current) || current < amount)
            {
                return false;
            }

            var next = current - amount;
            if (next == 0)
            {
                _items.Remove(itemId);
            }
            else
            {
                _items[itemId] = next;
            }

            _eventBus.Publish(new InventoryChangedEvent(itemId, -amount));
            return true;
        }

        public int GetItemCount(string itemId)
        {
            return _items.TryGetValue(itemId, out var count) ? count : 0;
        }

        public IReadOnlyDictionary<string, int> Snapshot()
        {
            return _items;
        }

        public void Restore(IReadOnlyDictionary<string, int> items)
        {
            _items.Clear();
            if (items == null)
            {
                return;
            }

            foreach (var kv in items)
            {
                if (!string.IsNullOrWhiteSpace(kv.Key) && kv.Value > 0)
                {
                    _items[kv.Key] = kv.Value;
                }
            }
        }
    }
}
