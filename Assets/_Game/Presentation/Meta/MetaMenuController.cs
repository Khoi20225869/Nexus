using Game.Bootstrap;
using UnityEngine;

namespace Game.Presentation.Meta
{
    public sealed class MetaMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private string characterIdToSwitch = "hero_knight";

        public void OnPlayPressed()
        {
            GameInstaller.Facade?.GoToGameplayScene();
        }

        public void OnOpenShopPressed()
        {
            TogglePanel(shopPanel);
        }

        public void OnOpenInventoryPressed()
        {
            TogglePanel(inventoryPanel);
        }

        public void OnOpenCharacterPressed()
        {
            TogglePanel(characterPanel);
        }

        public void OnSwitchCharacterPressed()
        {
            if (GameInstaller.Facade == null)
            {
                return;
            }

            GameInstaller.Facade.UnlockCharacter(characterIdToSwitch);
            GameInstaller.Facade.SwitchCharacter(characterIdToSwitch);
        }

        private static void TogglePanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }
    }
}
