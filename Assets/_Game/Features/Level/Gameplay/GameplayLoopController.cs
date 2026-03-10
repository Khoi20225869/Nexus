using Game.Bootstrap;
using Game.Features.Character.Config;
using Game.Features.Level.Config;
using UnityEngine;

namespace Game.Features.Level.Gameplay
{
    public sealed class GameplayLoopController : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private int levelCompleteGoldReward = 25;
        [SerializeField] private KeyCode completeLevelKey = KeyCode.K;

        private CharacterDatabase _characterDatabase;
        private LevelDatabase _levelDatabase;
        private GameObject _spawnedPlayer;

        private void Start()
        {
            if (GameInstaller.Services == null || GameInstaller.Facade == null)
            {
                return;
            }

            _characterDatabase = GameInstaller.Services.Resolve<CharacterDatabase>();
            _levelDatabase = GameInstaller.Services.Resolve<LevelDatabase>();
            SpawnActiveCharacter();
            EnsureCurrentLevelStarted();
        }

        private void Update()
        {
            if (Input.GetKeyDown(completeLevelKey))
            {
                CompleteLevelAndReturnMeta();
            }
        }

        public void CompleteLevelAndReturnMeta()
        {
            if (GameInstaller.Facade == null)
            {
                return;
            }

            GameInstaller.Facade.AddGold(levelCompleteGoldReward);
            GameInstaller.Facade.CompleteCurrentLevel();
            GameInstaller.Facade.GoToMetaScene();
        }

        private void SpawnActiveCharacter()
        {
            if (_characterDatabase == null)
            {
                return;
            }

            var activeId = GameInstaller.Facade.ActiveCharacterId;
            if (string.IsNullOrWhiteSpace(activeId))
            {
                return;
            }

            for (var i = 0; i < _characterDatabase.Characters.Count; i++)
            {
                var def = _characterDatabase.Characters[i];
                if (def == null || def.Id != activeId || def.Prefab == null)
                {
                    continue;
                }

                var spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
                _spawnedPlayer = Object.Instantiate(def.Prefab, spawnPos, Quaternion.identity);
                return;
            }
        }

        private void EnsureCurrentLevelStarted()
        {
            var currentLevelId = GameInstaller.Facade.CurrentLevelId;
            if (!string.IsNullOrWhiteSpace(currentLevelId))
            {
                return;
            }

            if (_levelDatabase == null || _levelDatabase.Levels.Count == 0)
            {
                return;
            }

            var fallbackLevel = _levelDatabase.Levels[0];
            if (fallbackLevel == null || string.IsNullOrWhiteSpace(fallbackLevel.Id))
            {
                return;
            }

            GameInstaller.Facade.UnlockLevel(fallbackLevel.Id);
            GameInstaller.Facade.StartLevel(fallbackLevel.Id);
        }
    }
}
