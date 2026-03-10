using System;
using System.Collections;
using Game.Bootstrap;
using Game.Features.Character.Config;
using Game.Features.Level.Config;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        private Coroutine _initializeRoutine;

        private void OnEnable()
        {
            _initializeRoutine = StartCoroutine(InitializeWhenReady());
        }

        private void OnDisable()
        {
            if (_initializeRoutine != null)
            {
                StopCoroutine(_initializeRoutine);
                _initializeRoutine = null;
            }
        }

        private IEnumerator InitializeWhenReady()
        {
            const float timeout = 2f;
            var elapsed = 0f;

            while ((GameInstaller.Services == null || GameInstaller.Facade == null) && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (GameInstaller.Services == null || GameInstaller.Facade == null)
            {
                Debug.LogWarning("GameplayLoopController: GameInstaller chua san sang, bo qua spawn player.");
                yield break;
            }

            _characterDatabase = GameInstaller.Services.Resolve<CharacterDatabase>();
            _levelDatabase = GameInstaller.Services.Resolve<LevelDatabase>();
            SpawnActiveCharacter();
            EnsureCurrentLevelStarted();
            _initializeRoutine = null;
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
            CharacterDefinition matchedDefinition = null;

            if (string.IsNullOrWhiteSpace(activeId) && _characterDatabase.Characters.Count > 0)
            {
                var firstCharacter = _characterDatabase.Characters[0];
                activeId = firstCharacter != null ? firstCharacter.Id : string.Empty;
            }

            for (var i = 0; i < _characterDatabase.Characters.Count; i++)
            {
                var def = _characterDatabase.Characters[i];
                if (def == null || def.Prefab == null)
                {
                    continue;
                }

                if (def.Id == activeId)
                {
                    matchedDefinition = def;
                    break;
                }
            }

            if (matchedDefinition == null)
            {
                for (var i = 0; i < _characterDatabase.Characters.Count; i++)
                {
                    var def = _characterDatabase.Characters[i];
                    if (def != null && def.Prefab != null)
                    {
                        matchedDefinition = def;
                        break;
                    }
                }
            }

            if (matchedDefinition == null)
            {
                Debug.LogWarning("GameplayLoopController: khong tim thay character prefab hop le de spawn.");
                return;
            }

            if (_spawnedPlayer != null)
            {
                Destroy(_spawnedPlayer);
            }

            var spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            _spawnedPlayer = UnityEngine.Object.Instantiate(matchedDefinition.Prefab, spawnPos, Quaternion.identity);
            EnsureRuntimeControllers(_spawnedPlayer);
            Debug.Log("GameplayLoopController: spawned player " + matchedDefinition.Id + " at " + spawnPos);
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

        private static void EnsureRuntimeControllers(GameObject playerInstance)
        {
            if (playerInstance == null)
            {
                return;
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
