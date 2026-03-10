namespace Game.Features.Inventory.Application
{
    public sealed class AddItemUseCase
    {
        private readonly IInventoryService _inventoryService;

        public AddItemUseCase(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public bool Execute(string itemId, int amount)
        {
            return _inventoryService.AddItem(itemId, amount);
        }
    }
}
