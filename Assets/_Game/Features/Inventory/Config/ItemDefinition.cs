using UnityEngine;

namespace Game.Features.Inventory.Config
{
    [CreateAssetMenu(menuName = "Game/Inventory/Item Definition", fileName = "ItemDefinition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public int MaxStack { get; private set; } = 99;
        [field: SerializeField] public int BasePrice { get; private set; } = 10;
    }
}
