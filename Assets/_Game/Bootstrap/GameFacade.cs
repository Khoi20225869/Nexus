using System.Collections.Generic;
using Game.Features.Character.Application;
using Game.Features.Economy.Application;
using Game.Features.Inventory.Application;
using Game.Features.Level.Application;
using Game.Features.Level.Config;
using Game.Features.Shop.Application;
using Game.Features.Shop.Domain;
using Game.Infrastructure.Save;
using Game.Infrastructure.Scenes;

namespace Game.Bootstrap
{
    public sealed class GameFacade
    {
        private readonly ICharacterService _characterService;
        private readonly IInventoryService _inventoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILevelService _levelService;
        private readonly IShopService _shopService;
        private readonly ISceneFlowService _sceneFlowService;
        private readonly IProgressSaveService _progressSaveService;
        private readonly LevelDatabase _levelDatabase;

        private readonly UnlockCharacterUseCase _unlockCharacterUseCase;
        private readonly SwitchCharacterUseCase _switchCharacterUseCase;
        private readonly AddItemUseCase _addItemUseCase;
        private readonly RemoveItemUseCase _removeItemUseCase;
        private readonly BuyItemUseCase _buyItemUseCase;
        private readonly StartLevelUseCase _startLevelUseCase;
        private readonly CompleteLevelUseCase _completeLevelUseCase;

        public GameFacade(
            ICharacterService characterService,
            IInventoryService inventoryService,
            ICurrencyService currencyService,
            ILevelService levelService,
            IShopService shopService,
            ISceneFlowService sceneFlowService,
            IProgressSaveService progressSaveService,
            LevelDatabase levelDatabase)
        {
            _characterService = characterService;
            _inventoryService = inventoryService;
            _currencyService = currencyService;
            _levelService = levelService;
            _shopService = shopService;
            _sceneFlowService = sceneFlowService;
            _progressSaveService = progressSaveService;
            _levelDatabase = levelDatabase;

            _unlockCharacterUseCase = new UnlockCharacterUseCase(_characterService);
            _switchCharacterUseCase = new SwitchCharacterUseCase(_characterService);
            _addItemUseCase = new AddItemUseCase(_inventoryService);
            _removeItemUseCase = new RemoveItemUseCase(_inventoryService);
            _buyItemUseCase = new BuyItemUseCase(_shopService);
            _startLevelUseCase = new StartLevelUseCase(_levelService);
            _completeLevelUseCase = new CompleteLevelUseCase(_levelService);
        }

        public int Gold => _currencyService.Gold;
        public string ActiveCharacterId => _characterService.ActiveCharacterId;
        public string CurrentLevelId => _levelService.CurrentLevelId;

        public IReadOnlyCollection<string> UnlockedCharacters => _characterService.UnlockedCharacterIds;
        public IReadOnlyCollection<string> UnlockedLevels => _levelService.UnlockedLevels;
        public IReadOnlyDictionary<string, int> Inventory => _inventoryService.Snapshot();

        public IReadOnlyList<ShopOffer> GetShopOffers() => _shopService.GetOffers();

        public void AddGold(int amount)
        {
            _currencyService.AddGold(amount);
            SaveProgress();
        }

        public bool UnlockCharacter(string characterId)
        {
            var ok = _unlockCharacterUseCase.Execute(characterId);
            if (ok)
            {
                SaveProgress();
            }

            return ok;
        }

        public bool SwitchCharacter(string characterId)
        {
            var ok = _switchCharacterUseCase.Execute(characterId);
            if (ok)
            {
                SaveProgress();
            }

            return ok;
        }

        public bool AddItem(string itemId, int amount)
        {
            var ok = _addItemUseCase.Execute(itemId, amount);
            if (ok)
            {
                SaveProgress();
            }

            return ok;
        }

        public bool RemoveItem(string itemId, int amount)
        {
            var ok = _removeItemUseCase.Execute(itemId, amount);
            if (ok)
            {
                SaveProgress();
            }

            return ok;
        }

        public PurchaseResult Buy(string offerId)
        {
            var result = _buyItemUseCase.Execute(offerId);
            if (result == PurchaseResult.Success)
            {
                SaveProgress();
            }

            return result;
        }

        public bool UnlockLevel(string levelId)
        {
            var ok = _levelService.UnlockLevel(levelId);
            if (ok)
            {
                SaveProgress();
            }

            return ok;
        }

        public bool StartLevel(string levelId)
        {
            var ok = _startLevelUseCase.Execute(levelId);
            if (ok)
            {
                SaveProgress();
            }

            return ok;
        }

        public void CompleteCurrentLevel()
        {
            _completeLevelUseCase.Execute();
            SaveProgress();
        }

        public void GoToMetaScene() => _sceneFlowService.LoadMetaScene();
        public void GoToGameplayScene()
        {
            var levelSceneName = ResolveCurrentLevelSceneName();
            if (!string.IsNullOrWhiteSpace(levelSceneName))
            {
                _sceneFlowService.LoadScene(levelSceneName);
                return;
            }

            var firstLevel = GetFirstLevelDefinition();
            if (firstLevel != null && !string.IsNullOrWhiteSpace(firstLevel.Id))
            {
                UnlockLevel(firstLevel.Id);
                if (StartLevel(firstLevel.Id) && !string.IsNullOrWhiteSpace(firstLevel.SceneName))
                {
                    _sceneFlowService.LoadScene(firstLevel.SceneName);
                    return;
                }
            }

            _sceneFlowService.LoadGameplayScene();
        }

        public void CompleteCurrentLevelAndAdvance()
        {
            var completedLevelId = _levelService.CurrentLevelId;
            CompleteCurrentLevel();

            var nextLevel = GetNextLevelDefinition(completedLevelId);
            if (nextLevel == null || string.IsNullOrWhiteSpace(nextLevel.Id))
            {
                GoToMetaScene();
                return;
            }

            UnlockLevel(nextLevel.Id);
            if (!StartLevel(nextLevel.Id))
            {
                GoToMetaScene();
                return;
            }

            var sceneName = nextLevel.SceneName;
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                GoToMetaScene();
                return;
            }

            _sceneFlowService.LoadScene(sceneName);
        }

        public void LoadScene(string sceneName) => _sceneFlowService.LoadScene(sceneName);

        public void SaveProgress()
        {
            _progressSaveService.Save(BuildSnapshot());
        }

        private PlayerProgressData BuildSnapshot()
        {
            var data = new PlayerProgressData
            {
                Gold = _currencyService.Gold,
                ActiveCharacterId = _characterService.ActiveCharacterId,
                CurrentLevelId = _levelService.CurrentLevelId,
                UnlockedCharacterIds = new List<string>(_characterService.UnlockedCharacterIds),
                UnlockedLevelIds = new List<string>(_levelService.UnlockedLevels)
            };

            foreach (var kv in _inventoryService.Snapshot())
            {
                data.InventoryEntries.Add(new InventoryEntryData { ItemId = kv.Key, Amount = kv.Value });
            }

            return data;
        }

        private string ResolveCurrentLevelSceneName()
        {
            var level = GetLevelDefinition(_levelService.CurrentLevelId);
            return level != null ? level.SceneName : string.Empty;
        }

        private LevelDefinition GetNextLevelDefinition(string levelId)
        {
            if (_levelDatabase == null || _levelDatabase.Levels == null || _levelDatabase.Levels.Count == 0)
            {
                return null;
            }

            for (var i = 0; i < _levelDatabase.Levels.Count; i++)
            {
                var level = _levelDatabase.Levels[i];
                if (level == null || level.Id != levelId)
                {
                    continue;
                }

                for (var j = i + 1; j < _levelDatabase.Levels.Count; j++)
                {
                    var nextLevel = _levelDatabase.Levels[j];
                    if (nextLevel != null)
                    {
                        return nextLevel;
                    }
                }

                return null;
            }

            return _levelDatabase.Levels[0];
        }

        private LevelDefinition GetLevelDefinition(string levelId)
        {
            return _levelDatabase != null ? _levelDatabase.GetById(levelId) : null;
        }

        private LevelDefinition GetFirstLevelDefinition()
        {
            if (_levelDatabase == null || _levelDatabase.Levels == null)
            {
                return null;
            }

            for (var i = 0; i < _levelDatabase.Levels.Count; i++)
            {
                if (_levelDatabase.Levels[i] != null)
                {
                    return _levelDatabase.Levels[i];
                }
            }

            return null;
        }
    }
}
