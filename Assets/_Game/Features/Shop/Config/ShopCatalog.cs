using System.Collections.Generic;
using Game.Features.Shop.Domain;
using UnityEngine;

namespace Game.Features.Shop.Config
{
    [CreateAssetMenu(menuName = "Game/Shop/Shop Catalog", fileName = "ShopCatalog")]
    public sealed class ShopCatalog : ScriptableObject
    {
        [field: SerializeField] public List<ShopOffer> Offers { get; private set; } = new();
    }
}
