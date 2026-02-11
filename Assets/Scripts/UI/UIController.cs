using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace ArmyClash.UI
{
    /// <summary>
    /// Main UI controller for menus and battle interface
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject battleUIPanel;
        [SerializeField] private GameObject endScreenPanel;

        private Core.BattleManager _battleManager;
        private Core.CombatController _combatController;

        [Inject]
        public void Construct(Core.BattleManager battleManager, Core.CombatController combatController)
        {
            _battleManager = battleManager;
            _combatController = combatController;
        }

        private void Start()
        {
            _battleManager.OnBattleEnded += OnBattleEnded;
            ShowMainMenu();
        }


        private void OnStartBattle()
        {
            ShowBattleUI();
            _battleManager.StartBattle();
        }

        private void OnRandomize()
        {
            _battleManager.RandomizeArmies();
        }

        private void OnBackToMenu()
        {
            _battleManager.ClearBattle();
            ShowMainMenu();
        }

        private void OnBattleEnded(Core.Team winner)
        {
            ShowEndScreen(winner);
        }

        private void ShowMainMenu()
        {
            mainMenuPanel?.SetActive(true);
            battleUIPanel?.SetActive(false);
            endScreenPanel?.SetActive(false);
        }

        public void ShowBattleUI()
        {
            mainMenuPanel?.SetActive(false);
            battleUIPanel?.SetActive(true);
            endScreenPanel?.SetActive(false);
        }

        private void ShowEndScreen(Core.Team winner)
        {
            mainMenuPanel?.SetActive(false);
            battleUIPanel?.SetActive(false);
            endScreenPanel?.SetActive(true);

        }

        private void OnDestroy()
        {
            if (_battleManager != null)
            {
                _battleManager.OnBattleEnded -= OnBattleEnded;
            }
        }
    }
}
