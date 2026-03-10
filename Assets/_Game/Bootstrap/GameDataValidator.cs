using System.Collections.Generic;
using Game.Features.Character.Config;
using Game.Features.Inventory.Config;
using Game.Features.Level.Config;
using Game.Features.Shop.Config;
using UnityEngine;

namespace Game.Bootstrap
{
    public sealed class GameDataValidator
    {
        private readonly CharacterDatabase _characterDatabase;
        private readonly ItemDatabase _itemDatabase;
        private readonly LevelDatabase _levelDatabase;
        private readonly ShopCatalog _shopCatalog;

        public GameDataValidator(
            CharacterDatabase characterDatabase,
            ItemDatabase itemDatabase,
            LevelDatabase levelDatabase,
            ShopCatalog shopCatalog)
        {
            _characterDatabase = characterDatabase;
            _itemDatabase = itemDatabase;
            _levelDatabase = levelDatabase;
            _shopCatalog = shopCatalog;
        }

        public bool ValidateAndLog()
        {
            var ok = true;
            var characterIds = new HashSet<string>();
            var itemIds = new HashSet<string>();
            var levelIds = new HashSet<string>();

            if (_characterDatabase != null)
            {
                foreach (var def in _characterDatabase.Characters)
                {
                    if (def == null || string.IsNullOrWhiteSpace(def.Id) || !characterIds.Add(def.Id))
                    {
                        ok = false;
                        Debug.LogWarning("[DataValidator] Invalid or duplicate character ID detected.");
                    }
                }
            }

            if (_itemDatabase != null)
            {
                foreach (var def in _itemDatabase.Items)
                {
                    if (def == null || string.IsNullOrWhiteSpace(def.Id) || !itemIds.Add(def.Id))
                    {
                        ok = false;
                        Debug.LogWarning("[DataValidator] Invalid or duplicate item ID detected.");
                    }
                }
            }

            if (_levelDatabase != null)
            {
                foreach (var def in _levelDatabase.Levels)
                {
                    if (def == null || string.IsNullOrWhiteSpace(def.Id) || !levelIds.Add(def.Id))
                    {
                        ok = false;
                        Debug.LogWarning("[DataValidator] Invalid or duplicate level ID detected.");
                    }
                }
            }

            if (_shopCatalog != null)
            {
                foreach (var offer in _shopCatalog.Offers)
                {
                    if (offer == null || string.IsNullOrWhiteSpace(offer.OfferId) || string.IsNullOrWhiteSpace(offer.ItemId))
                    {
                        ok = false;
                        Debug.LogWarning("[DataValidator] Invalid shop offer detected.");
                        continue;
                    }

                    if (itemIds.Count > 0 && !itemIds.Contains(offer.ItemId))
                    {
                        ok = false;
                        Debug.LogWarning($"[DataValidator] Shop offer item not found in ItemDatabase: {offer.ItemId}");
                    }
                }
            }

            return ok;
        }
    }
}
