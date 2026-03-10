using System;
using System.Linq;
using Game.Bootstrap;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Game.Presentation.Debugging
{
    public sealed class GameDebugController : MonoBehaviour
    {
        [SerializeField] private string testCharacterId = "hero_archer";
        [SerializeField] private string testLevelId = "level_02";
        [SerializeField] private int debugGoldAmount = 100;

        private GameFacade _facade;

        private void Start()
        {
            _facade = GameInstaller.Facade;
        }

        private void Update()
        {
            if (_facade == null)
            {
                return;
            }

            if (WasKeyPressedThisFrame(KeyCode.F1))
            {
                _facade.AddGold(debugGoldAmount);
            }

            if (WasKeyPressedThisFrame(KeyCode.F2))
            {
                var firstOffer = _facade.GetShopOffers().FirstOrDefault();
                if (firstOffer != null)
                {
                    _facade.Buy(firstOffer.OfferId);
                }
            }

            if (WasKeyPressedThisFrame(KeyCode.F3))
            {
                _facade.UnlockCharacter(testCharacterId);
                _facade.SwitchCharacter(testCharacterId);
            }

            if (WasKeyPressedThisFrame(KeyCode.F4))
            {
                _facade.UnlockLevel(testLevelId);
                _facade.StartLevel(testLevelId);
            }

            if (WasKeyPressedThisFrame(KeyCode.F5))
            {
                _facade.CompleteCurrentLevel();
            }

            if (WasKeyPressedThisFrame(KeyCode.F6))
            {
                _facade.GoToMetaScene();
            }

            if (WasKeyPressedThisFrame(KeyCode.F7))
            {
                _facade.GoToGameplayScene();
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
