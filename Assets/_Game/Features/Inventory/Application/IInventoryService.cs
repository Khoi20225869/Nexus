using System.Collections.Generic;

namespace Game.Features.Inventory.Application
{
    public interface IInventoryService
    {
        bool AddItem(string itemId, int amount);
        bool RemoveItem(string itemId, int amount);
        int GetItemCount(string itemId);
        IReadOnlyDictionary<string, int> Snapshot();
        void Restore(IReadOnlyDictionary<string, int> items);
    }
}
