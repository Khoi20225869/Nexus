using System;
using Game.Bootstrap;
using Game.Features.Character.Config;
using Game.Features.Level.Config;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Features.Level.Gameplay
{
    public sealed class GameplayLoopController : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private int levelCompleteGoldReward = 25;
        [SerializeField] private KeyCode completeLevelKey = KeyCode.K;
        [SerializeField] private RuntimeAnimatorController fallbackAnimatorController;

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
            if (WasKeyPressedThisFrame(completeLevelKey))
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
                _spawnedPlayer = UnityEngine.Object.Instantiate(def.Prefab, spawnPos, Quaternion.identity);
                EnsureRuntimeControllers(_spawnedPlayer, fallbackAnimatorController);
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

        private static void EnsureRuntimeControllers(GameObject playerInstance, RuntimeAnimatorController fallbackController)
        {
            if (playerInstance == null)
            {
                return;
            }

            var animator = playerInstance.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController == null)
            {
#if UNITY_EDITOR
                if (fallbackController == null)
                {
                    fallbackController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                        "Assets/Animators/Character Controller.controller");
                }
#endif
                if (fallbackController != null)
                {
                    animator.runtimeAnimatorController = fallbackController;
                }
            }

            if (playerInstance.GetComponent<PlayerLocomotionAnimatorDriver>() == null)
            {
                playerInstance.AddComponent<PlayerLocomotionAnimatorDriver>();
            }
        }

        private static bool WasKeyPressedThisFrame(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            if (!Enum.TryParse(keyCode.ToString(), ignoreCase: true, out Key key))
            {
                return false;
            }

            return keyboard[key].wasPressedThisFrame;
#else
            return Input.GetKeyDown(keyCode);
#endif
        }
    }
}
