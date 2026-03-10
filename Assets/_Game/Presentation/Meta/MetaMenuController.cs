using System.Collections;
using Game.Bootstrap;
using Game.Core.Events;
using Game.Features.Character.Config;
using Game.Features.Character.Domain;
using UnityEngine;

namespace Game.Presentation.Meta
{
    public sealed class MetaMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject inventoryPanel;

        [Header("Play Transition")]
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField] private float fadeDuration = 0.35f;

        private CharacterDatabase _characterDatabase;
        private IGameEventBus _eventBus;
        private Coroutine _initializeRoutine;
        private Coroutine _playTransitionRoutine;
        private int _selectedCharacterIndex;

        private void OnEnable()
        {
            HideAllPanels();
            _initializeRoutine = StartCoroutine(InitializeWhenReady());
        }

        private void OnDisable()
        {
            if (_initializeRoutine != null)
            {
                StopCoroutine(_initializeRoutine);
                _initializeRoutine = null;
            }

            if (_playTransitionRoutine != null)
            {
                StopCoroutine(_playTransitionRoutine);
                _playTransitionRoutine = null;
            }

            _eventBus?.Unsubscribe<CharacterSwitchedEvent>(OnCharacterSwitched);
        }

        public void OnPlayPressed()
        {
            if (_playTransitionRoutine != null)
            {
                return;
            }

            _playTransitionRoutine = StartCoroutine(PlayAndLoadGameplayScene());
        }

        public void OnOpenShopPressed()
        {
            OpenPanel(shopPanel);
        }

        public void OnOpenInventoryPressed()
        {
            OpenPanel(inventoryPanel);
        }

        public void OnOpenCharacterPressed()
        {
            OnSwitchCharacterPressed();
        }

        public void OnClosePanelsPressed()
        {
            HideAllPanels();
        }

        public void OnPreviousCharacterPressed()
        {
            if (_characterDatabase == null || _characterDatabase.Characters.Count == 0)
            {
                return;
            }

            _selectedCharacterIndex = WrapIndex(_selectedCharacterIndex - 1, _characterDatabase.Characters.Count);
            ApplySelectedCharacter();
        }

        public void OnNextCharacterPressed()
        {
            if (_characterDatabase == null || _characterDatabase.Characters.Count == 0)
            {
                return;
            }

            _selectedCharacterIndex = WrapIndex(_selectedCharacterIndex + 1, _characterDatabase.Characters.Count);
            ApplySelectedCharacter();
        }

        public void OnSwitchCharacterPressed()
        {
            if (_characterDatabase == null || _characterDatabase.Characters.Count == 0)
            {
                return;
            }

            _selectedCharacterIndex = WrapIndex(_selectedCharacterIndex + 1, _characterDatabase.Characters.Count);
            ApplySelectedCharacter();
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
                Debug.LogWarning("MetaMenuController: GameInstaller chua san sang.");
                yield break;
            }

            _characterDatabase = GameInstaller.Services.Resolve<CharacterDatabase>();
            _eventBus = GameInstaller.Services.Resolve<IGameEventBus>();
            _eventBus?.Subscribe<CharacterSwitchedEvent>(OnCharacterSwitched);

            SyncCharacterSelectionFromFacade();
            InitializeFadeOverlay();
            _initializeRoutine = null;
        }

        private IEnumerator PlayAndLoadGameplayScene()
        {
            HideAllPanels();

            if (fadeOverlay != null)
            {
                fadeOverlay.gameObject.SetActive(true);
                fadeOverlay.blocksRaycasts = true;

                var duration = Mathf.Max(0.01f, fadeDuration);
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    fadeOverlay.alpha = Mathf.Clamp01(elapsed / duration);
                    yield return null;
                }

                fadeOverlay.alpha = 1f;
            }

            GameInstaller.Facade?.GoToGameplayScene();
            _playTransitionRoutine = null;
        }

        private void OnCharacterSwitched(CharacterSwitchedEvent gameEvent)
        {
            SyncCharacterSelectionFromFacade();
        }

        private void SyncCharacterSelectionFromFacade()
        {
            if (_characterDatabase == null || _characterDatabase.Characters.Count == 0 || GameInstaller.Facade == null)
            {
                _selectedCharacterIndex = 0;
                return;
            }

            var activeCharacterId = GameInstaller.Facade.ActiveCharacterId;
            for (var i = 0; i < _characterDatabase.Characters.Count; i++)
            {
                var character = _characterDatabase.Characters[i];
                if (character != null && character.Id == activeCharacterId)
                {
                    _selectedCharacterIndex = i;
                    return;
                }
            }

            _selectedCharacterIndex = 0;
        }

        private void ApplySelectedCharacter()
        {
            if (GameInstaller.Facade == null || _characterDatabase == null || _characterDatabase.Characters.Count == 0)
            {
                return;
            }

            var character = _characterDatabase.Characters[_selectedCharacterIndex];
            if (character == null || string.IsNullOrWhiteSpace(character.Id))
            {
                return;
            }

            GameInstaller.Facade.UnlockCharacter(character.Id);
            GameInstaller.Facade.SwitchCharacter(character.Id);
        }

        private void InitializeFadeOverlay()
        {
            if (fadeOverlay == null)
            {
                return;
            }

            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
            fadeOverlay.interactable = false;
            fadeOverlay.gameObject.SetActive(true);
        }

        private void OpenPanel(GameObject targetPanel)
        {
            HideAllPanels();
            if (targetPanel != null)
            {
                targetPanel.SetActive(true);
            }
        }

        private void HideAllPanels()
        {
            SetPanelState(shopPanel, false);
            SetPanelState(inventoryPanel, false);
        }

        private static void SetPanelState(GameObject panel, bool isActive)
        {
            if (panel != null)
            {
                panel.SetActive(isActive);
            }
        }

        private static int WrapIndex(int value, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            var result = value % count;
            return result < 0 ? result + count : result;
        }
    }
}
