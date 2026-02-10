using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace ArmyClash.UI
{
    /// <summary>
    /// In-battle UI - shows unit counts, stats, and battle controls
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        [Header("Team Counters")]
        [SerializeField] private TextMeshProUGUI team1CountText;
        [SerializeField] private TextMeshProUGUI team2CountText;
        
        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI team1StatsText;
        [SerializeField] private TextMeshProUGUI team2StatsText;
        
        [Header("Battle Timer")]
        [SerializeField] private TextMeshProUGUI battleTimerText;
        
        [Header("Controls")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button speedUpButton;
        [SerializeField] private TextMeshProUGUI speedButtonText;
        
        [Header("Battle Info")]
        [SerializeField] private TextMeshProUGUI battleStatusText;
        [SerializeField] private GameObject pausePanel;

        private Core.CombatController _combatController;
        private float _battleStartTime;
        private bool _isPaused = false;
        private float _currentSpeed = 1f;

        [Inject]
        public void Construct(Core.CombatController combatController)
        {
            _combatController = combatController;
        }

        private void Start()
        {
            SetupButtons();
            _battleStartTime = Time.time;
        }

        private void SetupButtons()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(TogglePause);
            }

            if (speedUpButton != null)
            {
                speedUpButton.onClick.AddListener(CycleSpeed);
            }
        }

        private void Update()
        {
            UpdateUnitCounts();
            UpdateTeamStats();
            UpdateBattleTimer();
        }

        private void UpdateUnitCounts()
        {
            var team1Units = _combatController.GetUnitsOfTeam(Core.Team.Team1);
            var team2Units = _combatController.GetUnitsOfTeam(Core.Team.Team2);

            if (team1CountText != null)
            {
                team1CountText.text = $"Team 1: {team1Units.Count}";
            }

            if (team2CountText != null)
            {
                team2CountText.text = $"Team 2: {team2Units.Count}";
            }
        }

        private void UpdateTeamStats()
        {
            var team1Units = _combatController.GetUnitsOfTeam(Core.Team.Team1);
            var team2Units = _combatController.GetUnitsOfTeam(Core.Team.Team2);

            if (team1StatsText != null)
            {
                team1StatsText.text = GetTeamStatsText(team1Units);
            }

            if (team2StatsText != null)
            {
                team2StatsText.text = GetTeamStatsText(team2Units);
            }
        }

        private string GetTeamStatsText(System.Collections.Generic.List<Core.Unit> units)
        {
            if (units.Count == 0)
                return "Defeated!";

            float totalHP = 0;
            float totalMaxHP = 0;
            float totalATK = 0;

            foreach (var unit in units)
            {
                totalHP += unit.CurrentHP;
                totalMaxHP += unit.Stats.HP;
                totalATK += unit.Stats.ATK;
            }

            return $"HP: {totalHP:F0}/{totalMaxHP:F0}\n" +
                   $"ATK: {totalATK:F0}\n" +
                   $"Avg HP: {totalHP / units.Count:F0}";
        }

        private void UpdateBattleTimer()
        {
            if (battleTimerText != null)
            {
                float elapsed = Time.time - _battleStartTime;
                int minutes = (int)(elapsed / 60f);
                int seconds = (int)(elapsed % 60);
                battleTimerText.text = $"Time: {minutes:D2}:{seconds:D2}";
            }
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : _currentSpeed;
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(_isPaused);
            }

            UpdateBattleStatus();
        }

        private void CycleSpeed()
        {
            // Cycle through: 1x -> 2x -> 3x -> 1x
            switch (_currentSpeed)
            {
                case 1:
                    speedButtonText.text = ">>";
                    _currentSpeed = 2f;
                    break;
                case 2:
                    speedButtonText.text = ">>>";
                    _currentSpeed = 3f;
                    break;
                case 3:
                    speedButtonText.text = ">";
                    _currentSpeed = 1f;
                    break;
            }

            if (!_isPaused)
            {
                Time.timeScale = _currentSpeed;
            }
        }

        private void UpdateBattleStatus()
        {
            if (battleStatusText != null)
            {
                if (_isPaused)
                {
                    battleStatusText.text = "PAUSED";
                    battleStatusText.color = Color.yellow;
                }
                else
                {
                    battleStatusText.text = "BATTLE IN PROGRESS";
                    battleStatusText.color = Color.white;
                }
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _battleStartTime = Time.time;
            _isPaused = false;
            _currentSpeed = 1f;
            Time.timeScale = 1f;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            Time.timeScale = 1f; // Reset time scale
        }

        private void OnDestroy()
        {
            if (pauseButton != null)
                pauseButton.onClick.RemoveListener(TogglePause);
            
            if (speedUpButton != null)
                speedUpButton.onClick.RemoveListener(CycleSpeed);
          
            // Reset time scale on destroy
            Time.timeScale = 1f;
        }
    }
}
