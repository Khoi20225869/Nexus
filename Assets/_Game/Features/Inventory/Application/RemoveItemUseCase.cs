namespace Game.Features.Inventory.Application
{
    public sealed class RemoveItemUseCase
    {
        private readonly IInventoryService _inventoryService;

        public RemoveItemUseCase(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public bool Execute(string itemId, int amount)
        {
            return _inventoryService.RemoveItem(itemId, amount);
        }
    }
}
