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
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button randomizeButton;
        [SerializeField] private Button startBattleButton;
        
        [Header("Team Info")]
        [SerializeField] private TextMeshProUGUI team1InfoText;
        [SerializeField] private TextMeshProUGUI team2InfoText;
        
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject battleUI;

        private BattleManager _battleManager;

        [Inject]
        public void Construct(BattleManager battleManager)
        {
            _battleManager = battleManager;
        }

        private void Start()
        {

            randomizeButton.onClick.AddListener(OnRandomizeClicked);
            startBattleButton.onClick.AddListener(OnStartBattleClicked);
            _battleManager.OnUnitSpawned += OnUnitSpawned;
        }

        private void OnRandomizeClicked()
        {
            Debug.Log("[MainMenuUI] Randomize clicked");
            _battleManager.RandomizeArmies();
            UpdateTeamInfo();
        }

        private void OnStartBattleClicked()
        {
            Debug.Log("[MainMenuUI] Start Battle clicked");
            _battleManager.StartBattle();
            
            // Hide main menu, show battle UI
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }
            battleUI.SetActive(true);
        }

        private void OnUnitSpawned(Unit unit, Vector3 position, UnitShape shape)
        {          
            UpdateTeamInfo();
        }

        private void UpdateTeamInfo()
        {
            var team1Units = _battleManager.GetUnitsOfTeam(Core.Team.Team1);
            var team2Units = _battleManager.GetUnitsOfTeam(Core.Team.Team2);

            if (team1InfoText != null)
            {
                team1InfoText.text = $"Team 1: {team1Units.Count} units";
            }

            if (team2InfoText != null)
            {
                team2InfoText.text = $"Team 2: {team2Units.Count} units";
            }
        }

        public void Show()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
            
            UpdateTeamInfo();
        }

        public void Hide()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            startBattleButton.onClick.RemoveListener(OnStartBattleClicked);
            randomizeButton.onClick.RemoveListener(OnRandomizeClicked);
            _battleManager.OnUnitSpawned -= OnUnitSpawned;
        }
    }
}
