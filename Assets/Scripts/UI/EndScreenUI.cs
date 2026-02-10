using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace ArmyClash.UI
{
    /// <summary>
    /// End screen UI - displays battle results and statistics
    /// </summary>
    public class EndScreenUI : MonoBehaviour
    {
        [Header("Victory Display")]
        [SerializeField] private TextMeshProUGUI _winnerTitleText;
        [SerializeField] private Image _winnerBackground;
        [SerializeField] private Color _team1Color = Color.blue;
        [SerializeField] private Color _team2Color = Color.red;
        
        [Header("Battle Statistics")]
        [SerializeField] private TextMeshProUGUI _battleStatsText;
        [SerializeField] private TextMeshProUGUI _survivorsText;
        
        [Header("MVP Display")]
        [SerializeField] private GameObject _mvpPanel;
        [SerializeField] private TextMeshProUGUI _mvpText;
        
        [Header("Buttons")]
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _quitButton;
        
        [Header("Animation")]
        [SerializeField] private Animator _screenAnimator;
        [SerializeField] private ParticleSystem _confettiEffect;

        private Core.Team _winner;
        private BattleStatistics _stats;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (_playAgainButton != null)
            {
                _playAgainButton.onClick.AddListener(OnPlayAgain);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(OnMainMenu);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuit);
            }
        }

        /// <summary>
        /// Display battle results
        /// </summary>
        public void ShowResults(Core.Team winner, BattleStatistics stats)
        {
            _winner = winner;
            _stats = stats;

            UpdateWinnerDisplay();
            UpdateStatistics();
            UpdateMVP();
            
            PlayVictoryAnimation();
            
            gameObject.SetActive(true);
        }

        private void UpdateWinnerDisplay()
        {
            if (_winnerTitleText != null)
            {
                _winnerTitleText.text = $"{_winner} WINS!";
            }

            if (_winnerBackground != null)
            {
                _winnerBackground.color = _winner == Core.Team.Team1 
                    ? _team1Color 
                    : _team2Color;
            }
        }

        private void UpdateStatistics()
        {
            if (_battleStatsText != null && _stats != null)
            {
                _battleStatsText.text = 
                    $"<b>Battle Statistics</b>\n\n" +
                    $"Duration: {FormatTime(_stats.BattleDuration)}\n" +
                    $"Total Damage Dealt: {_stats.TotalDamageDealt:F0}\n" +
                    $"Units Defeated: {_stats.UnitsDefeated}\n" +
                    $"Longest Battle: {FormatTime(_stats.LongestBattleDuration)}\n" +
                    $"Quickest Kill: {FormatTime(_stats.QuickestKill)}";
            }

            if (_survivorsText != null && _stats != null)
            {
                _survivorsText.text = 
                    $"<b>Survivors</b>\n" +
                    $"{_winner}: {_stats.SurvivingUnits} units\n" +
                    $"Average HP: {_stats.AverageSurvivorHP:F0}";
            }
        }

        private void UpdateMVP()
        {
            if (_mvpPanel != null && _mvpText != null && _stats != null && _stats.MVPUnit != null)
            {
                var mvp = _stats.MVPUnit;
                _mvpPanel.SetActive(true);
                
                _mvpText.text = 
                    $"<b>MVP UNIT</b>\n\n" +
                    $"Type: {mvp.Shape} {mvp.Color} {mvp.Size}\n" +
                    $"Damage Dealt: {_stats.MVPDamageDealt:F0}\n" +
                    $"Kills: {_stats.MVPKills}\n" +
                    $"Survived: {(mvp.IsAlive ? "Yes" : "No")}";
            }
            else
            {
                if (_mvpPanel != null)
                    _mvpPanel.SetActive(false);
            }
        }

        private void PlayVictoryAnimation()
        {
            if (_screenAnimator != null)
            {
                _screenAnimator.SetTrigger("Show");
            }

            if (_confettiEffect != null)
            {
                _confettiEffect.Play();
            }
        }

        private string FormatTime(float seconds)
        {
            int minutes = (int)(seconds / 60f);
            int secs = (int)(seconds % 60);
            return $"{minutes:D2}:{secs:D2}";
        }

        private void OnPlayAgain()
        {
            // Will be handled by UIController
            Hide();
        }

        private void OnMainMenu()
        {
            // Will be handled by UIController
            Hide();
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_playAgainButton != null)
                _playAgainButton.onClick.RemoveListener(OnPlayAgain);
            
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.RemoveListener(OnMainMenu);
            
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuit);
        }
    }

    /// <summary>
    /// Data class for battle statistics
    /// </summary>
    [System.Serializable]
    public class BattleStatistics
    {
        public float BattleDuration;
        public float TotalDamageDealt;
        public int UnitsDefeated;
        public float LongestBattleDuration;
        public float QuickestKill;
        public int SurvivingUnits;
        public float AverageSurvivorHP;
        public Core.Unit MVPUnit;
        public float MVPDamageDealt;
        public int MVPKills;
    }
}
