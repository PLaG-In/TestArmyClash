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
        [SerializeField] private TextMeshProUGUI _team1CountText;
        [SerializeField] private TextMeshProUGUI _team2CountText;
        [SerializeField] private Image _team1CountBackground;
        [SerializeField] private Image _team2CountBackground;
        
        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI _team1StatsText;
        [SerializeField] private TextMeshProUGUI _team2StatsText;
        
        [Header("Battle Timer")]
        [SerializeField] private TextMeshProUGUI _battleTimerText;
        
        [Header("Controls")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _speedUpButton;
        [SerializeField] private Slider _gameSpeedSlider;
        [SerializeField] private TextMeshProUGUI _speedText;
        
        [Header("Battle Info")]
        [SerializeField] private TextMeshProUGUI _battleStatusText;
        [SerializeField] private GameObject _pausePanel;

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
            SetupSpeedControls();
            _battleStartTime = Time.time;
        }

        private void SetupButtons()
        {
            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(TogglePause);
            }

            if (_speedUpButton != null)
            {
                _speedUpButton.onClick.AddListener(CycleSpeed);
            }
        }

        private void SetupSpeedControls()
        {
            if (_gameSpeedSlider != null)
            {
                _gameSpeedSlider.minValue = 0.5f;
                _gameSpeedSlider.maxValue = 3f;
                _gameSpeedSlider.value = 1f;
                _gameSpeedSlider.onValueChanged.AddListener(OnSpeedChanged);
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

            if (_team1CountText != null)
            {
                _team1CountText.text = $"Team 1: {team1Units.Count}";
                
                // Color based on unit count
                if (_team1CountBackground != null)
                {
                    float ratio = team1Units.Count / 20f;
                    _team1CountBackground.color = Color.Lerp(Color.red, Color.green, ratio);
                }
            }

            if (_team2CountText != null)
            {
                _team2CountText.text = $"Team 2: {team2Units.Count}";
                
                if (_team2CountBackground != null)
                {
                    float ratio = team2Units.Count / 20f;
                    _team2CountBackground.color = Color.Lerp(Color.red, Color.green, ratio);
                }
            }
        }

        private void UpdateTeamStats()
        {
            var team1Units = _combatController.GetUnitsOfTeam(Core.Team.Team1);
            var team2Units = _combatController.GetUnitsOfTeam(Core.Team.Team2);

            if (_team1StatsText != null)
            {
                _team1StatsText.text = GetTeamStatsText(team1Units);
            }

            if (_team2StatsText != null)
            {
                _team2StatsText.text = GetTeamStatsText(team2Units);
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
            if (_battleTimerText != null)
            {
                float elapsed = Time.time - _battleStartTime;
                int minutes = (int)(elapsed / 60f);
                int seconds = (int)(elapsed % 60);
                _battleTimerText.text = $"Time: {minutes:D2}:{seconds:D2}";
            }
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : _currentSpeed;
            
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(_isPaused);
            }

            UpdateBattleStatus();
        }

        private void CycleSpeed()
        {
            // Cycle through: 1x -> 2x -> 3x -> 1x
            if (_currentSpeed == 1f)
                _currentSpeed = 2f;
            else if (_currentSpeed == 2f)
                _currentSpeed = 3f;
            else
                _currentSpeed = 1f;

            if (!_isPaused)
            {
                Time.timeScale = _currentSpeed;
            }

            if (_gameSpeedSlider != null)
            {
                _gameSpeedSlider.value = _currentSpeed;
            }

            UpdateSpeedText();
        }

        private void OnSpeedChanged(float value)
        {
            _currentSpeed = value;
            
            if (!_isPaused)
            {
                Time.timeScale = _currentSpeed;
            }

            UpdateSpeedText();
        }

        private void UpdateSpeedText()
        {
            if (_speedText != null)
            {
                _speedText.text = $"Speed: {_currentSpeed:F1}x";
            }
        }

        private void UpdateBattleStatus()
        {
            if (_battleStatusText != null)
            {
                if (_isPaused)
                {
                    _battleStatusText.text = "PAUSED";
                    _battleStatusText.color = Color.yellow;
                }
                else
                {
                    _battleStatusText.text = "BATTLE IN PROGRESS";
                    _battleStatusText.color = Color.white;
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
            if (_pauseButton != null)
                _pauseButton.onClick.RemoveListener(TogglePause);
            
            if (_speedUpButton != null)
                _speedUpButton.onClick.RemoveListener(CycleSpeed);
            
            if (_gameSpeedSlider != null)
                _gameSpeedSlider.onValueChanged.RemoveListener(OnSpeedChanged);

            // Reset time scale on destroy
            Time.timeScale = 1f;
        }
    }
}
