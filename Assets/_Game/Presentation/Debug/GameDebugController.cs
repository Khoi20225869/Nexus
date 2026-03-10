using System.Linq;
using Game.Bootstrap;
using UnityEngine;

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

            if (Input.GetKeyDown(KeyCode.F1))
            {
                _facade.AddGold(debugGoldAmount);
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                var firstOffer = _facade.GetShopOffers().FirstOrDefault();
                if (firstOffer != null)
                {
                    _facade.Buy(firstOffer.OfferId);
                }
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                _facade.UnlockCharacter(testCharacterId);
                _facade.SwitchCharacter(testCharacterId);
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                _facade.UnlockLevel(testLevelId);
                _facade.StartLevel(testLevelId);
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                _facade.CompleteCurrentLevel();
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                _facade.GoToMetaScene();
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                _facade.GoToGameplayScene();
            }
        }
    }
}
