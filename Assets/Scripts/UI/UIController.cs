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
        [Header("Main Menu")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private Button _startBattleButton;
        [SerializeField] private Button _randomizeButton;

        [Header("Battle UI")]
        [SerializeField] private GameObject _battleUIPanel;
        [SerializeField] private TextMeshProUGUI _team1CountText;
        [SerializeField] private TextMeshProUGUI _team2CountText;

        [Header("End Screen")]
        [SerializeField] private GameObject _endScreenPanel;
        [SerializeField] private TextMeshProUGUI _winnerText;
        [SerializeField] private Button _backToMenuButton;

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
            SetupButtons();
            _battleManager.OnBattleEnded += OnBattleEnded;
            ShowMainMenu();
        }

        private void SetupButtons()
        {
            _startBattleButton?.onClick.AddListener(OnStartBattle);
            _randomizeButton?.onClick.AddListener(OnRandomize);
            _backToMenuButton?.onClick.AddListener(OnBackToMenu);
        }

        private void Update()
        {
            if (_battleUIPanel != null && _battleUIPanel.activeSelf)
            {
                UpdateBattleUI();
            }
        }

        private void UpdateBattleUI()
        {
            int team1Count = _combatController.GetUnitsOfTeam(Core.Team.Team1).Count;
            int team2Count = _combatController.GetUnitsOfTeam(Core.Team.Team2).Count;

            if (_team1CountText != null)
            {
                _team1CountText.text = $"Team 1: {team1Count}";
            }

            if (_team2CountText != null)
            {
                _team2CountText.text = $"Team 2: {team2Count}";
            }
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
            _mainMenuPanel?.SetActive(true);
            _battleUIPanel?.SetActive(false);
            _endScreenPanel?.SetActive(false);
        }

        private void ShowBattleUI()
        {
            _mainMenuPanel?.SetActive(false);
            _battleUIPanel?.SetActive(true);
            _endScreenPanel?.SetActive(false);
        }

        private void ShowEndScreen(Core.Team winner)
        {
            _mainMenuPanel?.SetActive(false);
            _battleUIPanel?.SetActive(false);
            _endScreenPanel?.SetActive(true);

            if (_winnerText != null)
            {
                _winnerText.text = $"{winner} Wins!";
            }
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
