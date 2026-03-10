using Game.Bootstrap;
using Game.Core.Events;
using Game.Features.Character.Config;
using Game.Features.Character.Domain;
using System.Collections;
using UnityEngine;

namespace Game.Presentation.Meta
{
    public sealed class MetaCharacterPreviewSpawner : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;

        private CharacterDatabase _characterDatabase;
        private IGameEventBus _eventBus;
        private GameObject _spawnedPreview;
        private Coroutine _initializeRoutine;

        private void OnEnable()
        {
            _initializeRoutine = StartCoroutine(InitializeWhenReady());
        }

        private void OnDestroy()
        {
            if (_initializeRoutine != null)
            {
                StopCoroutine(_initializeRoutine);
                _initializeRoutine = null;
            }

            _eventBus?.Unsubscribe<CharacterSwitchedEvent>(OnCharacterSwitched);
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
                Debug.LogWarning("MetaCharacterPreviewSpawner: GameInstaller chua san sang. Neu ban dang mo truc tiep Meta.unity, hay chay qua Bootstrap.unity.");
                yield break;
            }

            _characterDatabase = GameInstaller.Services.Resolve<CharacterDatabase>();
            _eventBus = GameInstaller.Services.Resolve<IGameEventBus>();
            _eventBus?.Subscribe<CharacterSwitchedEvent>(OnCharacterSwitched);
            SpawnActiveCharacter();
            _initializeRoutine = null;
        }

        private void OnCharacterSwitched(CharacterSwitchedEvent gameEvent)
        {
            SpawnCharacter(gameEvent.CharacterId);
        }

        private void SpawnActiveCharacter()
        {
            var activeCharacterId = GameInstaller.Facade.ActiveCharacterId;
            if (string.IsNullOrWhiteSpace(activeCharacterId) && _characterDatabase != null && _characterDatabase.Characters.Count > 0)
            {
                var firstCharacter = _characterDatabase.Characters[0];
                activeCharacterId = firstCharacter != null ? firstCharacter.Id : string.Empty;
            }

            SpawnCharacter(activeCharacterId);
        }

        private void SpawnCharacter(string characterId)
        {
            if (_characterDatabase == null || _characterDatabase.Characters.Count == 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(characterId))
            {
                var firstCharacter = _characterDatabase.Characters[0];
                characterId = firstCharacter != null ? firstCharacter.Id : string.Empty;
            }

            CharacterDefinition matchedDefinition = null;

            for (var i = 0; i < _characterDatabase.Characters.Count; i++)
            {
                var definition = _characterDatabase.Characters[i];
                if (definition == null || definition.Prefab == null)
                {
                    continue;
                }

                if (definition.Id == characterId)
                {
                    matchedDefinition = definition;
                    break;
                }
            }

            if (matchedDefinition == null)
            {
                for (var i = 0; i < _characterDatabase.Characters.Count; i++)
                {
                    var definition = _characterDatabase.Characters[i];
                    if (definition != null && definition.Prefab != null)
                    {
                        matchedDefinition = definition;
                        break;
                    }
                }
            }

            if (matchedDefinition == null)
            {
                Debug.LogWarning("MetaCharacterPreviewSpawner: khong tim thay prefab hop le trong CharacterDatabase.");
                return;
            }

            if (_spawnedPreview != null)
            {
                Destroy(_spawnedPreview);
            }

            var parent = spawnPoint != null ? spawnPoint : transform;
            _spawnedPreview = Instantiate(matchedDefinition.Prefab, parent);
            _spawnedPreview.name = matchedDefinition.Prefab.name + " Preview";
            _spawnedPreview.transform.localPosition = Vector3.zero;
            _spawnedPreview.transform.localRotation = Quaternion.identity;
            _spawnedPreview.transform.localScale = Vector3.one;

            Debug.Log("MetaCharacterPreviewSpawner: spawned preview for " + matchedDefinition.Id);
        }
    }
}
