using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using ArmyClash.Core;

namespace ArmyClash.UI
{
    /// <summary>
    /// Main menu UI - allows army randomization and battle start
    /// Fixed: Separate Prepare and Start buttons
    /// </summary>
    public class EndBattleUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button mainMenuButton;

        [Header("Win Info")]
        [SerializeField] private TextMeshProUGUI winInfoText;

        [Header("Win Info")]
        [SerializeField] private GameObject mainMenu;

        private void Start()
        {
            mainMenuButton.onClick.AddListener(OnBackToMainMenu);
        }

        public void SetWinnerText(Core.Team winner)
        {
            winInfoText.text = $"Battle ended! Winner: {winner}";
        }

        private void OnBackToMainMenu()
        {
            mainMenu.SetActive(true);
            gameObject.SetActive(false);
        }


        private void OnDestroy()
        {
            mainMenuButton.onClick.RemoveListener(OnBackToMainMenu);
        }
    }
}
