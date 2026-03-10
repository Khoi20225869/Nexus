using System.Collections.Generic;
using UnityEngine;

namespace Game.Features.Inventory.Config
{
    [CreateAssetMenu(menuName = "Game/Inventory/Item Database", fileName = "ItemDatabase")]
    public sealed class ItemDatabase : ScriptableObject
    {
        [field: SerializeField] public List<ItemDefinition> Items { get; private set; } = new();

        public ItemDefinition GetById(string id)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (item != null && item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
