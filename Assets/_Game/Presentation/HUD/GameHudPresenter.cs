using Game.Bootstrap;
using Game.Core.Events;
using Game.Features.Character.Domain;
using Game.Features.Economy.Domain;
using Game.Features.Inventory.Domain;
using Game.Features.Level.Domain;
using Game.Features.Shop.Domain;
using UnityEngine;

namespace Game.Presentation.HUD
{
    public sealed class GameHudPresenter : MonoBehaviour
    {
        [SerializeField] private bool verboseLog = true;

        private IGameEventBus _eventBus;

        private void OnEnable()
        {
            if (GameInstaller.Services == null)
            {
                return;
            }

            _eventBus = GameInstaller.Services.Resolve<IGameEventBus>();
            _eventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            _eventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            _eventBus.Subscribe<ItemPurchasedEvent>(OnItemPurchased);
            _eventBus.Subscribe<CharacterSwitchedEvent>(OnCharacterSwitched);
            _eventBus.Subscribe<LevelStartedEvent>(OnLevelStarted);
            _eventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);

            if (verboseLog)
            {
                var facade = GameInstaller.Facade;
                Debug.Log($"[HUD] Ready | Gold: {facade.Gold} | Character: {facade.ActiveCharacterId}");
            }
        }

        private void OnDisable()
        {
            if (_eventBus == null)
            {
                return;
            }

            _eventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
            _eventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            _eventBus.Unsubscribe<ItemPurchasedEvent>(OnItemPurchased);
            _eventBus.Unsubscribe<CharacterSwitchedEvent>(OnCharacterSwitched);
            _eventBus.Unsubscribe<LevelStartedEvent>(OnLevelStarted);
            _eventBus.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        private void OnGoldChanged(GoldChangedEvent gameEvent)
        {
            if (verboseLog)
            {
                Debug.Log($"[HUD] Gold: {gameEvent.CurrentGold} (delta {gameEvent.Delta})");
            }
        }

        private void OnInventoryChanged(InventoryChangedEvent gameEvent)
        {
            if (verboseLog)
            {
                Debug.Log($"[HUD] Inventory change: {gameEvent.ItemId} ({gameEvent.Delta})");
            }
        }

        private void OnItemPurchased(ItemPurchasedEvent gameEvent)
        {
            if (verboseLog)
            {
                Debug.Log($"[HUD] Purchased offer {gameEvent.OfferId} -> {gameEvent.ItemId} x{gameEvent.Amount}");
            }
        }

        private void OnCharacterSwitched(CharacterSwitchedEvent gameEvent)
        {
            if (verboseLog)
            {
                Debug.Log($"[HUD] Active character: {gameEvent.CharacterId}");
            }
        }

        private void OnLevelStarted(LevelStartedEvent gameEvent)
        {
            if (verboseLog)
            {
                Debug.Log($"[HUD] Level started: {gameEvent.LevelId}");
            }
        }

        private void OnLevelCompleted(LevelCompletedEvent gameEvent)
        {
            if (verboseLog)
            {
                Debug.Log($"[HUD] Level completed: {gameEvent.LevelId}");
            }
        }
    }
}
