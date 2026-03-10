using System.Collections.Generic;
using Game.Core.Events;
using Game.Core.Services;
using Game.Features.Character.Application;
using Game.Features.Character.Config;
using Game.Features.Economy.Application;
using Game.Features.Inventory.Application;
using Game.Features.Inventory.Config;
using Game.Features.Level.Application;
using Game.Features.Level.Config;
using Game.Features.Shop.Application;
using Game.Features.Shop.Config;
using Game.Infrastructure.Save;
using Game.Infrastructure.Scenes;
using UnityEngine;

namespace Game.Bootstrap
{
    public sealed class GameInstaller : MonoBehaviour
    {
        [Header("Starting State")]
        [SerializeField] private string starterCharacterId = "hero_knight";
        [SerializeField] private string starterLevelId = "level_01";
        [SerializeField] private int startingGold = 100;

        [Header("Configs")]
        [SerializeField] private ShopCatalog shopCatalog;
        [SerializeField] private CharacterDatabase characterDatabase;
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private LevelDatabase levelDatabase;
        [SerializeField] private GameBootstrapConfig bootstrapConfig;

        public static IServiceRegistry Services { get; private set; }
        public static GameFacade Facade { get; private set; }

        private void Awake()
        {
            if (Services != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            var serviceRegistry = new ServiceRegistry();
            var eventBus = new GameEventBus();
            var currencyService = new CurrencyService(eventBus, startingGold);
            var inventoryService = new InventoryService(eventBus);
            var characterService = new CharacterService(eventBus, starterCharacterId);
            var levelService = new LevelService(eventBus, starterLevelId);
            var shopService = new ShopService(shopCatalog, currencyService, inventoryService, eventBus);

            var metaSceneName = bootstrapConfig != null ? bootstrapConfig.MetaSceneName : string.Empty;
            var gameplaySceneName = bootstrapConfig != null ? bootstrapConfig.GameplaySceneName : string.Empty;
            var sceneFlowService = new UnitySceneFlowService(metaSceneName, gameplaySceneName);

            var saveFileName = bootstrapConfig != null ? bootstrapConfig.SaveFileName : "player_progress.json";
            var progressSaveService = new JsonProgressSaveService(saveFileName);

            RestoreProgress(progressSaveService, currencyService, characterService, inventoryService, levelService);

            var dataValidator = new GameDataValidator(characterDatabase, itemDatabase, levelDatabase, shopCatalog);
            dataValidator.ValidateAndLog();

            var facade = new GameFacade(
                characterService,
                inventoryService,
                currencyService,
                levelService,
                shopService,
                sceneFlowService,
                progressSaveService);

            serviceRegistry.Register<IGameEventBus>(eventBus);
            serviceRegistry.Register<ICurrencyService>(currencyService);
            serviceRegistry.Register<IInventoryService>(inventoryService);
            serviceRegistry.Register<ICharacterService>(characterService);
            serviceRegistry.Register<ILevelService>(levelService);
            serviceRegistry.Register<IShopService>(shopService);
            serviceRegistry.Register<ISceneFlowService>(sceneFlowService);
            serviceRegistry.Register<IProgressSaveService>(progressSaveService);
            serviceRegistry.Register(facade);

            if (characterDatabase != null)
            {
                serviceRegistry.Register(characterDatabase);
            }

            if (itemDatabase != null)
            {
                serviceRegistry.Register(itemDatabase);
            }

            if (levelDatabase != null)
            {
                serviceRegistry.Register(levelDatabase);
            }

            Services = serviceRegistry;
            Facade = facade;

            if (bootstrapConfig != null && bootstrapConfig.AutoLoadMetaOnStart)
            {
                sceneFlowService.LoadMetaScene();
            }
        }

        private static void RestoreProgress(
            IProgressSaveService progressSaveService,
            ICurrencyService currencyService,
            ICharacterService characterService,
            IInventoryService inventoryService,
            ILevelService levelService)
        {
            var data = progressSaveService.Load();
            if (data == null)
            {
                return;
            }

            currencyService.SetGold(data.Gold);
            characterService.Restore(data.ActiveCharacterId, data.UnlockedCharacterIds);

            var inventoryMap = new Dictionary<string, int>();
            foreach (var entry in data.InventoryEntries)
            {
                if (!string.IsNullOrWhiteSpace(entry.ItemId) && entry.Amount > 0)
                {
                    inventoryMap[entry.ItemId] = entry.Amount;
                }
            }

            inventoryService.Restore(inventoryMap);
            levelService.Restore(data.CurrentLevelId, data.UnlockedLevelIds);
        }
    }
}
